using System;
using System.Collections.Generic;
using System.Text;

namespace OSRS_DataWorker.Models
{
    class ReturnedTrend
    {
        public ReturnedItem item {get;set;}
    }

    public class ReturnedItem
    {
        public string icon { get; set; }
        public string icon_large { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public string typeIcon { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool members { get; set; }
        public ReturnedPriceTrend current { get; set; }
        public ReturnedPriceTrend today { get; set; }
        public ReturnedChangeTrend day30 { get; set; }
        public ReturnedChangeTrend day90 { get; set; }
        public ReturnedChangeTrend day180 { get; set; }
    }

    public class ReturnedChangeTrend
    {
        public string trend { get; set; }
        public string change { get; set; }
    }

    public class ReturnedPriceTrend
    {
        public string trend { get; set; }
        public string price { get; set; }
    }
}
