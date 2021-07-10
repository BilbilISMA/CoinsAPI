namespace FraktonCoins.Models.DTOs.Responses
{
    public class CoinData
    {
        public string Id { get; set; }
        public int? Rank { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? Supply { get; set; }
        public decimal? MaxSupply { get; set; }
    }
}