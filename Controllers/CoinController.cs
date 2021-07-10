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

        /// <summary>
        /// Gets all the available coins data from the external microservice
        /// </summary>
        /// <returns>A JSON containing id, rank, symbol, name, supply and maxSupply for each coin</returns>
        [HttpGet]
        [Route("Coins")]
        public async Task<IActionResult> GetCoins()
        {
            //Calling the handler that manages the external calls and data mapping
            var coins = await _apiHandler.GetCoins(_apiConfig.BaseUrl, _apiConfig.CoinsEndpoint);
            return Ok(coins);
        }

        /// <summary>
        /// Gets the favorite coins of the authenticated user using the user's claims
        /// </summary>
        /// <returns>A JSON containing id, rank, symbol, name, supply and maxSupply for each favorite coin</returns>
        [HttpGet]
        [Route("FavoriteCoins")]
        public async Task<IActionResult> GetFavoriteCoins()
        {
            try
            {
                //Accessing the user claims to get the user data.
                var authenticatedUser = await _userManager.FindByIdAsync(User.Claims.Where(_ => _.Type == "Id").FirstOrDefault().Value);
                if (authenticatedUser == null)
                    return NotFound();

                //Getting the data from SQLite db
                var favoriteCoinIds = _context.FavoriteCoins.Where(_ => _.UserId.Equals(authenticatedUser.Id)).Select(_ => _.CoinId).ToList();
                //Fetching all the available coins data from external API to cross-check and provide the actual coin data 
                var apiResponse = await _apiHandler.GetCoins(_apiConfig.BaseUrl, _apiConfig.CoinsEndpoint);
                var favoriteCoins = apiResponse.Where(_ => favoriteCoinIds.Contains(_.Id)).ToList();
                return Ok(favoriteCoins);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }            
        }

        /// <summary>
        /// Sets(adds or removes) the favorite coin for the authenticate user. 
        /// If the coin by the provided id is already a favorite coin, it will be removed, otherwise it will be added as a favorite coin.
        /// </summary>
        /// <param name="coinId">The coin id from the external API</param>
        /// <returns>A status code depending on the outcome</returns>
        [HttpPost]
        [Route("SetFavoriteCoin")]
        public async Task<IActionResult> SetFavoriteCoin(string coinId)
        {
            try
            {
                if (string.IsNullOrEmpty(coinId))
                    return BadRequest();
                //Accessing authenticated user's claim and getting its data (the check isn't really necessary in this case since the endpoint is already protected)
                var authenticatedUser = await _userManager.FindByIdAsync(User.Claims.Where(_ => _.Type == "Id").FirstOrDefault().Value);
                if (authenticatedUser == null)
                    return NotFound();

                //Getting the potential fav coin with the provided id
                var existingFavoriteCoin = _context.FavoriteCoins.SingleOrDefault(_ => _.UserId.Equals(authenticatedUser.Id) && _.CoinId == coinId);
                if (existingFavoriteCoin != null)//If it's already fav, remove it
                {
                    _context.FavoriteCoins.Remove(existingFavoriteCoin);
                }
                else//otherwise, added it as fav coin
                {
                    await _context.FavoriteCoins.AddAsync(
                    new FavoriteCoinData
                    {
                        CoinId = coinId,
                        UserId = authenticatedUser.Id
                    });
                }   
                //save the changes on db
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