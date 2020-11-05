using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blazor_OSRS_Helper.Shared.Models
{
    public class RSItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool incomplete { get; set; }
        public bool members { get; set; }
        public bool tradeable { get; set; }

        [JsonProperty("tradeable_on_ge")]
        public bool tradeableOnGE { get; set; }

        public bool stackable { get; set; }
        public int? stacked { get; set; }
        public bool noted { get; set; }
        public bool noteable { get; set; }

        [JsonProperty("linked_id_item")]
        public int? linkedIdItem { get; set; }

        [JsonProperty("linked_id_noted")]
        public int? linkedIdNoted { get; set; }

        [JsonProperty("linked_id_placeholder")]
        public int? linkedIdPlaceholder { get; set; }

        public bool placeholder { get; set; }
        public bool equipable { get; set; }

        [JsonProperty("equipable_by_player")]
        public bool equipableByPlayer { get; set; }

        [JsonProperty("equipable_weapon")]
        public bool equipableWeapon { get; set; }

        public int cost { get; set; }
        public int? lowalch { get; set; }
        public int? highalch { get; set; }
        public decimal? weight { get; set; }

        [JsonProperty("buy_limit")]
        public int? buyLimit { get; set; }

        [JsonProperty("quest_item")]
        public bool questItem { get; set; }

        [JsonProperty("release_date")]
        public DateTime? releaseDate { get; set; }

        public bool duplicate { get; set; }
        public string examine { get; set; }
        public string icon { get; set; }

        [JsonProperty("wiki_url")]
        public string wikiUrl { get; set; }

        [JsonProperty("wiki_exchange")]
        public string wikiExchange { get; set; }

        public RSEquipmentStat equipment { get; set; }
        public RSWeaponStat weapon { get; set; }

        // Added Properties

        public string description { get; set; }
        public bool? isCraftable { get; set; }
        public List<RSCraftingMethod> craftingMethods { get; set; }
    }
}
