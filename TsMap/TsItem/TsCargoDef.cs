using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsMap.Utils;

namespace TsMap.TsItem
{
    public class TsCargoDef
    {
        [SiiUnitFieldAttribute(Name = "cargo_data")]
        public string InGameId { get; set; }
        public string Name { get; set; }
        [SiiUnitFieldAttribute(Name = "name")]
        //[JsonIgnore]
        public string LocalizationToken { get; set; }

        public Dictionary<string, string> LocalizedNames { get; set; } = new Dictionary<string, string>();

        [SiiUnitFieldAttribute(Name = "adr_class")]
        public int AdrClass { get; set; }
        [SiiUnitFieldAttribute(Name = "volume")]
        public decimal Volume { get; set; }
        [SiiUnitFieldAttribute(Name = "mass")]
        public decimal Mass { get; set; }
        [SiiUnitFieldAttribute(Name = "unit_reward_per_km")]
        public decimal UnitRewardPerKm { get; set; }
        [SiiUnitFieldAttribute(Name = "unit_load_time")]
        public int UnitLoadTime { get; set; }
        [SiiUnitFieldAttribute(Name = "group[]")]
        public List<string> Groups { get; set; } = new List<string>();
        [SiiUnitFieldAttribute(Name = "body_types[]")]
        public List<string> BodyTypes { get; set; } = new List<string>();
        [SiiUnitFieldAttribute(Name = "overweight")]
        public bool Overweight { get; set; }
        [SiiUnitFieldAttribute(Name = "valuable")]
        public bool Valueable { get; set; }
        [SiiUnitFieldAttribute(Name = "fragility")]
        public decimal Fragility { get; set; }
    }
}
