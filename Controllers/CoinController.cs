using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using FraktonCoins.Data;
using FraktonCoins.Handlers;
using FraktonCoins.Models;
using FraktonCoins.Models.DTOs.Requests;

namespace FraktonCoins.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CoinController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApiHandler _apiHandler;
        private readonly ApiConfig _apiConfig;
        public CoinController(
            ApiDbContext context,
            UserManager<IdentityUser> userManager,
            ApiHandler apiHandler,
            IOptionsMonitor<ApiConfig> optionsMonitor)
        {   
            _context = context;
            _userManager = userManager;
            _apiHandler = apiHandler;
            _apiConfig = optionsMonitor.CurrentValue;
        }

        [HttpGet]
        [Route("Coins")]
        public async Task<IActionResult> GetCoins()
        {
            var coins = await _apiHandler.GetCoins(_apiConfig.BaseUrl, _apiConfig.CoinsEndpoint);
            return Ok(coins);
        }

        [HttpGet]
        [Route("FavoriteCoins")]
        public async Task<IActionResult> GetFavoriteCoins()
        {
            try
            {
                var authenticatedUser = await _userManager.FindByIdAsync(User.Claims.Where(_ => _.Type == "Id").FirstOrDefault().Value);
                if (authenticatedUser == null)
                    return NotFound();

                var favoriteCoinIds = _context.FavoriteCoins.Where(_ => _.UserId.Equals(authenticatedUser.Id)).Select(_ => _.CoinId).ToList();
                var apiResponse = await _apiHandler.GetCoins(_apiConfig.BaseUrl, _apiConfig.CoinsEndpoint);
                var favoriteCoins = apiResponse.Where(_ => favoriteCoinIds.Contains(_.Id)).ToList();
                return Ok(favoriteCoins);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
        }

        [HttpPost]
        [Route("SetFavoriteCoin")]
        public async Task<IActionResult> SetFavoriteCoin(string coinId)
        {
            try
            {
                if (string.IsNullOrEmpty(coinId))
                    return BadRequest();

                var authenticatedUser = await _userManager.FindByIdAsync(User.Claims.Where(_ => _.Type == "Id").FirstOrDefault().Value);
                if (authenticatedUser == null)
                    return NotFound();

                var existingFavoriteCoin = _context.FavoriteCoins.SingleOrDefault(_ => _.UserId.Equals(authenticatedUser.Id) && _.CoinId == coinId);
                if (existingFavoriteCoin != null)
                {
                    _context.FavoriteCoins.Remove(existingFavoriteCoin);
                }
                else
                {
                    await _context.FavoriteCoins.AddAsync(
                    new FavoriteCoinData
                    {
                        CoinId = coinId,
                        UserId = authenticatedUser.Id
                    });
                }   

                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }            
        }
    }
}