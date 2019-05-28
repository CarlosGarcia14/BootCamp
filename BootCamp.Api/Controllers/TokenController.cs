using BootCamp.DataAccess;
using BootCamp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BootCamp.Api.Controllers
{
    /// <summary>
    /// Token Controller
    /// </summary>
    [Route("api/Token")]
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        /// <summary>
        /// Token Controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="db"></param>
        public TokenController(IConfiguration configuration, AppDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        /// <summary>
        /// Create Token
        /// </summary>
        /// <param name="request"> Login Information</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetToken([FromBody] LoginInfo request)
        {
            var account = _db.LoginInfos.FirstOrDefault(
                m => m.Username == request.Username && m.Password == request.Password);

            if (account != null)
            {
                var userclaim = new[] { new Claim(ClaimTypes.Name, request.Username) };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "https://localhost:44399",
                    audience: "https://localhost:44399",
                    claims: userclaim,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds);


                var _refreshTokenObj = new RefreshToken
                {
                    Username = request.Username,
                    Refreshtoken = Guid.NewGuid().ToString()
                };
                _db.RefreshTokens.Add(_refreshTokenObj);
                _db.SaveChanges();

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = _refreshTokenObj.Refreshtoken
                });
            }

            return Unauthorized();
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="refreshToken">Refresh Token GUID</param>
        /// <returns></returns>
        [HttpPost("{refreshToken}/refresh")]
        public IActionResult RefreshToken([FromRoute]string refreshToken)
        {
            var _refreshToken = _db.RefreshTokens.SingleOrDefault(m => m.Refreshtoken == refreshToken);

            if (_refreshToken == null)
            {
                return NotFound("Refresh token not found");
            }
            var userclaim = new[] { new Claim(ClaimTypes.Name, _refreshToken.Username) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:44399",
                audience: "https://localhost:44399",
                claims: userclaim,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds);

            _refreshToken.Refreshtoken = Guid.NewGuid().ToString();
            _db.RefreshTokens.Update(_refreshToken);
            _db.SaveChanges();

            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = _refreshToken.Refreshtoken
                });
        }
    }
}
