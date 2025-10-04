using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.JourneyPlan.Model.Interfaces
{
    public interface IBeatHistory : IBaseModelV2
    {
        public string UserJourneyUID { get; set; }
        public string OrgUID { get; set; }
        public string RouteUID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string JobPositionUID { get; set; }
        public string LoginId { get; set; }
        public DateTime VisitDate { get; set; }
        public string LocationUID { get; set; }
        public string PlannedStartTime { get; set; }
        public string PlannedEndTime { get; set; }
        public int PlannedStoreVisits { get; set; }
        public int UnPlannedStoreVisits { get; set; }
        public int ZeroSalesStoreVisits { get; set; }
        public int MSLStoreVisits { get; set; }
        public int SkippedStoreVisits { get; set; }
        public int ActualStoreVisits { get; set; }
        public decimal Coverage { get; set; }
        public decimal ACoverage { get; set; }
        public decimal TCoverage { get; set; }
        public string InvoiceStatus { get; set; }
        public string Notes { get; set; }
        public DateTime InvoiceFinalizationDate { get; set; }
        public string RouteWHOrgUID { get; set; }
        public DateTime CFDTime { get; set; }
        public bool HasAuditCompleted { get; set; }
        public string WHStockAuditUID { get; set; }
        public string DefaultJobPositionUID { get; set; }
        public string UserJourneyVehicleUID { get; set; }
        public int YearMonth { get; set; }
    }
}
