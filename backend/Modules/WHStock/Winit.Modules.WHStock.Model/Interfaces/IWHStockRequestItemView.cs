using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IWHStockRequestItemView
    {
        public string UID { get; set; }
        public string RequestCode { get; set; }
        public string RequestType { get; set; }
        public string RouteUID { get; set; }
        public string RouteCode { get; set; }
        public string RouteName { get; set; }
        public string SourceCode { get; set; }
        public string OrgUID { get; set; }
        public string SourceName { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime RequestedTime { get; set; }
        public DateTime RequiredByDate { get; set; }
        public DateTime ModifiedTime { get; set; }

        public string SourceOrgUID { get; set; }
        public string SourceWHUID { get; set; }
        public string TargetOrgUID { get; set; }
        public string TargetWHUID { get; set; }
        public string WareHouseUID { get; set; }
        public int YearMonth { get; set; }

        // New fields for organization and warehouse names
        public string SourceOrgCode { get; set; }
        public string SourceOrgName { get; set; }
        public string SourceWHCode { get; set; }
        public string SourceWHName { get; set; }
        public string TargetOrgCode { get; set; }
        public string TargetOrgName { get; set; }
        public string TargetWHCode { get; set; }
        public string TargetWHName { get; set; }

    }
}
