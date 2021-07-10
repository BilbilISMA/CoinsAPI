using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using FraktonCoins.Models;
using FraktonCoins.Models.DTOs.Responses;

namespace FraktonCoins.Handlers
{
    public class ApiHandler
    {
        public async Task<List<CoinData>> GetCoins(string apiBaseUrl, string apiEndPoint)
        {
            var coinCapApiClient = new RestClient(apiBaseUrl);
            var request = new RestRequest(apiEndPoint, Method.GET, DataFormat.Json);

            CoinApiResponse response = await coinCapApiClient.GetAsync<CoinApiResponse>(request);
            return response.Data;
        }
    }
}
