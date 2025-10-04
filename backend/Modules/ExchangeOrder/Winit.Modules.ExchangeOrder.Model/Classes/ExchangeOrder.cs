using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ExchangeOrder.Model.Interfaces;

namespace Winit.Modules.ExchangeOrder.Model.Classes
{
    public class ExchangeOrder : BaseModel, IExchangeOrder
    {
        public string ExchangeOrderNumber { get; set; }
        public string DraftOrderNumber { get; set; }
        public string StoreVisitHistory { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string StoreUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string Status { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public int LineCount { get; set; }
        public int QtyCount { get; set; }
        public string Notes { get; set; }
        public bool IsOffline { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string ReferenceNumber { get; set; }
        public string DeliveredByOrgUID { get; set; }
    }

   
}
