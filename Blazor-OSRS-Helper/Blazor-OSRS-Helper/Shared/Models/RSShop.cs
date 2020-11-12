using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSShop
    {
        public string name { get; set; }
        public string location { get; set; }
        public string specialty { get; set; }
        public string owner { get; set; }
        public bool isMember { get; set; }
        public List<RSStock> stock { get; set; }
    }
}
