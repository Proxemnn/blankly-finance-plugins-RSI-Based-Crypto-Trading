namespace Crybot.TradingUtils
{
    public class MarketInfo
    {
        public MarketInfo(string market, string chartUrl, int quantity, int leverage)
        {
            Market = market;
            ChartUrl = chartUrl;
            Quantity = quantity;
            Leverage = leverage;
        }

        public string Market { get; set; }

        public string ChartUrl { get; set; }

        public int Quantity { get; set; }

        public int Leverage { get; set; }
    }
}