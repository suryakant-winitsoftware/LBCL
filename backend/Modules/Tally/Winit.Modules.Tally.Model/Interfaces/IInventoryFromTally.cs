using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface IInventoryFromTally : IBaseModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Units { get; set; }
        public string LastRecordDate { get; set; }
        public string OpeningBalance { get; set; }
        public string OpeningRate { get; set; }
        public string StockGroup { get; set; }
        public string Parent { get; set; }
        public string GSTDetails { get; set; }
        public string RemoteAltGUID { get; set; }
        public string DistributorCode { get; set; }
        public string DistributorOrgUID { get; set; }
    }
}
