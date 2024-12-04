using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsMap.Utils;

namespace TsMap.TsItem
{
    public class TsCompanyDef
    {
        [SiiUnitFieldAttribute(Name = "company_permanent")]
        public string InGameId { get; set; }
        [SiiUnitFieldAttribute(Name = "name")]
        public string Name { get; set; }       

        [SiiUnitFieldAttribute(Name = "sort_name")]
        //[JsonIgnore]
        public string SortName { get; set; }

        [SiiUnitFieldAttribute(Name = "trailer_look")]
        //[JsonIgnore]
        public string TrailerLook { get; set; }
    }
}
