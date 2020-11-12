using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSStock
    {
        public int itemId { get; set; }
        public int? numberInStock { get; set; }
        public int? priceSoldAt { get; set; }
        public int? priceBoughtAt { get; set; }
        public bool isInfinite { get; set; }
        public bool isNotCoin { get; set; }
    }
}
