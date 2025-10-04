using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallyInventoryMaster : BaseModel, ITallyInventoryMaster
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Units { get; set; }
        public string LastRecordDate { get; set; }
        public string OpeningBalance { get; set; }
        public string OpeningRate { get; set; }
        public string StockGroup { get; set; }
        public string Parent { get; set; }
        public string GstDetails { get; set; }
        public string RemoteAltGuid { get; set; }
        public string DistributorCode { get; set; }
    }
}
