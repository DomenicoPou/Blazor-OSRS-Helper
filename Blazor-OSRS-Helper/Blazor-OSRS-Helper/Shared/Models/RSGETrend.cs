using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSGETrend
    {
        public int itemId { get; set; }
        public int current { get; set; }
        public Dictionary<int, float?> changes { get; set; }
    }
}
