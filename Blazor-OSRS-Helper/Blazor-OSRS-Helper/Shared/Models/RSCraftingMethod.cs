using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSCraftingMethod
    {
        public Dictionary<string, int> skillRequirments { get; set; }
        public decimal experienceGained { get; set; }
        public int quantityMade { get; set; }
        public List<int> tools { get; set; }
        public List<string> facilities { get; set; }
        public bool isMember { get; set; }
        public int ticks { get; set; }
        public Dictionary<int, int> materials { get; set; }
    }
}
