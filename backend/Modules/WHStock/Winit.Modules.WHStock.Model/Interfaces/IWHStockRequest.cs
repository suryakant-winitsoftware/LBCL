using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.WHStock.Model.Interfaces
{
    public interface IWHStockRequest : IBaseModel
    {

        public string CompanyUID { get; set; }
        public string SourceOrgUID { get; set; }
        public string SourceWHUID { get; set; }
        public string TargetOrgUID { get; set; }
        public string TargetWHUID { get; set; }
        public string Code { get; set; }
        public string RequestType { get; set; }
        public string RequestByEmpUID { get; set; }
        public string JobPositionUID { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string StockType { get; set; }
        public string RouteUID { get; set; }
        public string OrgUID { get; set; }
        public string WareHouseUID { get; set; }
        public int YearMonth { get; set; }
        public ActionType ActionType { get; set; }
    }
}
