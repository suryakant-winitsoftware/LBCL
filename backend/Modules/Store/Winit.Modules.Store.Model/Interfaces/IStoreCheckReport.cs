using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreCheckReport:IBaseModel
    {
        public string RouteCode { get; set; }
        public string RouteName { get; set; }
        public string SalesmanCode { get; set; }
        public string SalesmanName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ImagePath { get; set; }
        public DateTime Date { get; set; }
        public List<IStoreCheckReportItem> Items { get; set; } 

    }
    public interface IStoreCheckReportItem
    {
        public string SKU { get; set; }
        public string UOM { get; set; }
        public decimal SuggestedQty { get; set; }
        public decimal StoreQty { get; set; }
        public decimal BackstoreQty { get; set; }
        public decimal ToFillQty { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDRESelected { get; set; }
    }

}
