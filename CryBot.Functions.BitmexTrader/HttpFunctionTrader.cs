using CryBot.Functions.Utils;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;

using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CryBot.Functions.BitmexTrader
{
    public static class HttpFunctionTrader
    {
        private static readonly List<MarketInfo> MarketCharts = new List<MarketInfo>
        {
            //new MarketInfo("BCHU18", "https://www.tradingview.com/chart/z977J1a7/", 2, 20, 4, 2),
            new MarketInfo("XBTUSD", "https://www.tradingview.com/chart/WpYk6xkq/", 1500, 50, 0, 1.4M),
            new MarketInfo("XRPU18", "https://www.tradingview.com/chart/S1CVQjyz/", 2000, 15, 6, 3.5M),
            new MarketInfo("ETHUSD", "https://www.tradingview.com/chart/RkkgaHHm/", 2000, 35, 1, 1.5M),
            new MarketInfo("ADAU18", "https://www.tradingview.com/chart/W0NboM7z/", 10000, 15, 8, 3)
        };

        [FunctionName("HttpFunctionTrader")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                CryptoTrader.Timestamp = DateTime.Now.Ticks;
                Logger.Init(log);
                Logger.Log($"Started function at {DateTime.UtcNow}");
                AzureContainerManager azureContainerManager = new AzureContainerManager();
                var status = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "status", StringComparison.OrdinalIgnoreCase) == 0)
                    .Value;

                if (status == "loaded")
                {
                    foreach (var marketChart in MarketCharts)
                    {
                        Logger.Log($"Retrieving signal for {marketChart.Market}");
                        var cryptoTrader = new CryptoTrader();
                        await cryptoTrader.RetrieveAndProcessSignal(Environment.GetEnvironmentVariable("containerUrl"), marketChart);
                    }
                    azureContainerManager.StopImageAnalyzer();
                }
                Logger.Log($"Finished function at {DateTime.UtcNow}");
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
            return req.CreateResponse(HttpStatusCode.OK, "invalid");
        }
    }

}
