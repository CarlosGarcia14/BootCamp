using BootCamp.Entities;
using Microsoft.EntityFrameworkCore;

namespace BootCamp.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<LoginInfo> LoginInfos { get; set; }
    }
}
