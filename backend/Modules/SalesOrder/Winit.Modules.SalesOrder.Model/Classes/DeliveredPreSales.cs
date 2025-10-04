using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.Model.UIClasses;
using Winit.Modules.SalesOrder.Model.UIInterfaces;

namespace Winit.Modules.SalesOrder.Model.Classes
{
    public class DeliveredPreSales : IDeliveredPreSales
    {
        public string FranchiseCode { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string OrgUID { get; set; }
        public string EmpUID { get; set; }
        public string SalesOrderUID { get; set; }
        public string SalesOrderNumber { get; set; }
        public string DraftOrderNumber { get; set; }
        public string StoreNumber { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public int SKUCount { get; set; }
       
        public string RouteName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string EmpName { get; set; }
      
    }
}
