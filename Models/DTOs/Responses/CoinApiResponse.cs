using System.Collections.Generic;

namespace FraktonCoins.Models.DTOs.Responses
{
    public class CoinApiResponse
    {
        public List<CoinData> Data { get; set; }
        public long Timestamp { get; set; }
    }
}
