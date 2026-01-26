using System;
using System.Threading.Tasks;
using Bitmex.NET;
using Bitmex.NET.Models;

namespace Crybot.TradingUtils
{
    public class BitmexClient
    {
        private readonly IBitmexApiService _bitmexApiService;

        public BitmexClient()
        {
            _bitmexApiService = CreateBitmexClient();
        }

        private IBitmexApiService CreateBitmexClient()
        {
            var bitmexAuthorization = new BitmexAuthorization
            {
                BitmexEnvironment = Environment.GetEnvironmentVariable("bitmexEnvironment") == "Test"
                    ? BitmexEnvironment.Test
                    : BitmexEnvironment.Prod,
                Key = Environment.GetEnvironmentVariable("bitmexTestnetKey"),
                Secret = Environment.GetEnvironmentVariable("bitmexTestnetSecret")
            };
            var bitmexApiService = BitmexApiService.CreateDefaultApi(bitmexAuthorization);
            return bitmexApiService;
        }

        public async Task<string> GoLong(MarketInfo marketInfo)
        {
            await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.ClosePositionByMarket(marketInfo.Market));
            await SetLeverage(marketInfo.Market, marketInfo.Leverage);
            var buyPrice = await GetCurrentPrice(marketInfo.Market);
            var orderDto = await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder,
                OrderPOSTRequestParams.CreateSimpleLimit(marketInfo.Market, marketInfo.Quantity, buyPrice, OrderSide.Buy));
            var stopPrice = Math.Round((decimal)orderDto.Price * 1.005M, 0);
            //await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.CreateSimpleHidenLimit(marketInfo.Market, marketInfo.Quantity, buyPrice - 50, OrderSide.Buy));
            await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.CreateSimpleHidenLimit(marketInfo.Market, marketInfo.Quantity, stopPrice, OrderSide.Sell));
            return $"Bought {orderDto.OrderQty} {marketInfo.Market} at {orderDto.Price}, stop order set to {stopPrice}";
        }

        public async Task<string> GoShort(MarketInfo marketInfo)
        {
            await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.ClosePositionByMarket(marketInfo.Market));

            await SetLeverage(marketInfo.Market, marketInfo.Leverage);
            var sellPrice = await GetCurrentPrice(marketInfo.Market);
            //create limit order
            var orderDto = await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder,
                OrderPOSTRequestParams.CreateSimpleLimit(marketInfo.Market, marketInfo.Quantity, sellPrice, OrderSide.Sell));
            var stopPrice = Math.Round((decimal)orderDto.Price * 0.995M, 0);
            //create stop order
            //await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.CreateSimpleHidenLimit(marketInfo.Market, marketInfo.Quantity, sellPrice + 50, OrderSide.Sell));
            await _bitmexApiService.Execute(BitmexApiUrls.Order.PostOrder, OrderPOSTRequestParams.CreateSimpleHidenLimit(marketInfo.Market, marketInfo.Quantity, stopPrice, OrderSide.Buy));
            //create 
            return $"Sold {orderDto.OrderQty} {marketInfo.Market} at {orderDto.Price}, stop order set to {stopPrice}";
        }

        private async Task SetLeverage(string market, int leverage)
        {
            var positionLeveragePostRequestParams = new PositionLeveragePOSTRequestParams();
            positionLeveragePostRequestParams.Leverage = leverage;
            positionLeveragePostRequestParams.Symbol = market;
            await _bitmexApiService.Execute(BitmexApiUrls.Position.PostPositionLeverage, positionLeveragePostRequestParams);
        }

        private async Task<decimal> GetCurrentPrice(string market)
        {
            var bitcoinOrderBook = await _bitmexApiService.Execute(BitmexApiUrls.OrderBook.GetOrderBookL2,
                new OrderBookL2GETRequestParams { Depth = 1, Symbol = market });
            var buyPrice = Math.Round((bitcoinOrderBook[0].Price + bitcoinOrderBook[1].Price) / 2);
            return buyPrice;
        }
    }
}
