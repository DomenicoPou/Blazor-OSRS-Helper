using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSWeaponStat
    {
        [JsonProperty("attack_speed")]
        public int attackSpeed { get; set; }

        [JsonProperty("weapon_type")]
        public string weaponType { get; set; }

        public List<RSStances> stances { get; set; }
    }
}
