using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSStances
    {
        [JsonProperty("combat_style")]
        public string combatStyle { get; set; }

        [JsonProperty("attack_type")]
        public string attackType { get; set; }

        [JsonProperty("attack_style")]
        public string attackStyle { get; set; }

        public string experience { get; set; }
        public string boosts { get; set; }
    }
}
