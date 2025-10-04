using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StockAudit.Model.Classes
{
    public class WHStockAuditItemView: BaseModel, Winit.Modules.StockAudit.Model.Interfaces.IWHStockAuditItemView
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string CompanyUID { get; set; }
        public string WareHouseUID { get; set; }
        public string OrgUID { get; set; }
        public string AuditBy { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string JobPositionUID { get; set; }
        public string Remarks { get; set; }
        public string ParentUID { get; set; }
        public string AuditNumber { get; set; }
        public string ParentWarehouseUID { get; set; }
        public string ReferenceUID { get; set; }
        public string UserJourneyUID { get; set; }
        public string AdjustmentStatus { get; set; }
        public string Source { get; set; }
        public string WHStockAuditTemplateUID { get; set; }
        public bool HasUnloadStock { get; set; }
        public string RouteUID { get; set; }
        public int SS { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }
        public bool IsLastRoute { get; set; }
        public DateTime LastAuditTime { get; set; }
        public string? CalculationStatus { get; set; }
        public DateTime? CalculationStartTime { get; set; }
        public DateTime? CalculationEndTime { get; set; }
        public ActionType ActionType { get; set; }
    }
}
