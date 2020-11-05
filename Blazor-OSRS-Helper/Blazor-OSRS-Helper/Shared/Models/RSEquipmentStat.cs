using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSEquipmentStat
    {

        [JsonProperty("attack_stab")]
        public int attackStab { get; set; }

        [JsonProperty("attack_slash")]
        public int attackSlash { get; set; }

        [JsonProperty("attack_crush")]
        public int attackCrush { get; set; }

        [JsonProperty("attack_magic")]
        public int attackMagic { get; set; }

        [JsonProperty("attack_ranged")]
        public int attackRanged { get; set; }

        [JsonProperty("defence_stab")]
        public int defenceStab { get; set; }

        [JsonProperty("defence_slash")]
        public int defenceSlash { get; set; }

        [JsonProperty("defence_crush")]
        public int defenceCrush { get; set; }

        [JsonProperty("defence_magic")]
        public int defenceMagic { get; set; }

        [JsonProperty("defence_ranged")]
        public int defenceRanged { get; set; }

        [JsonProperty("melee_strength")]
        public int meleeStrength { get; set; }

        [JsonProperty("ranged_strength")]
        public int rangedStrength { get; set; }

        [JsonProperty("magic_damage")]
        public int magicDamage { get; set; }

        public int prayer { get; set; }
        public string slot { get; set; }
        public Dictionary<string, int> requirements { get; set; }
    }
}
