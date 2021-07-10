using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FraktonCoins.Models;
using FraktonCoins.Models.DTOs.Requests;

namespace FraktonCoins.Data
{
    public class ApiDbContext : IdentityDbContext
    {
        public virtual DbSet<FavoriteCoinData> FavoriteCoins {get;set;}
        public virtual DbSet<RefreshToken> RefreshTokens {get;set;}

        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            : base(options)
        {
            
        }
    }
}