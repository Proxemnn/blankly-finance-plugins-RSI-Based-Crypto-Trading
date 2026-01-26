using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Crybot.TradingUtils
{
    public class CryptoTrader
    {
        public async Task RetrieveAndProcessSignal(string url, MarketInfo marketInfo)
        {
            var bitmapAnalyzer = new BitmapAnalyzer();
            var signalType = await bitmapAnalyzer.GetLastSignal(url, marketInfo);

            await CheckSignalWithLast(signalType, marketInfo);

            var table = await GetSignalsTable();
            var insertOperation = TableOperation.Insert(new Signal { SignalType = signalType.ToString().ToLower(), Time = DateTime.UtcNow, Market = marketInfo.Market });
            await table.ExecuteAsync(insertOperation);
        }

        private async Task CheckSignalWithLast(SignalType signalType, MarketInfo marketInfo)
        {
            var table = await GetSignalsTable();
            var tableOperation = TableOperation.Retrieve<Signal>("bitmex", "");
            await table.ExecuteAsync(tableOperation);
            var query = new TableQuery<Signal>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "bitmex"))
                .Where(TableQuery.GenerateFilterCondition("Market", QueryComparisons.Equal, marketInfo.Market));
            var results = await table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            var lastResult = results.Results.OrderBy(o => o.Time).LastOrDefault();
            if (lastResult?.SignalType != signalType.ToString().ToLower() && signalType != SignalType.None)
            {
                string message;
                var bitmexClient = new BitmexClient();
                if (signalType == SignalType.Bullish)
                {
                    message = await bitmexClient.GoLong(marketInfo);
                }
                else if(signalType == SignalType.Bearish)
                {
                    message = await bitmexClient.GoShort(marketInfo);
                }
                else
                {
                    return;
                }
                Logger.Log(message);

                await new Mailman().SendMailAsync($"Signal got changed to {signalType} for {marketInfo.Market}.\n {message}");
            }
        }

        private async Task<CloudTable> GetSignalsTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("storageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("bitmex");

            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}