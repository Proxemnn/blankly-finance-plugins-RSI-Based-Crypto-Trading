using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Crybot.TradingUtils
{
    public class Signal : TableEntity
    {
        private DateTime _time;

        public Signal()
        {
            PartitionKey = "bitmex";
        }

        public string SignalType { get; set; }

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                RowKey = _time.ToString("F");
            }
        }

        public string Market { get; set; }
    }
}