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
    public class ViewPreSales : IViewPreSales
    {
        public string? CustomerNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? PONumber { get; set; }
        public string? SalesOrderNumber { get; set; }
        public string? DraftOrderNumber { get; set; }   
        public string? SalesREP { get; set; }
        public string? OrderType { get; set; }
        public int? TotalSKUCount { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentType { get; set; }
        public string? RouteName { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public decimal QtyCount { get; set; }
        public string? Notes { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal? TotalTax { get; set; }
        public decimal? NetAmount { get; set; }
        public string? ManualDocketNumber { get; set; }
        public string? Source { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ReferenceUID { get; set; }
        public List<ISKUViewPreSales>? sKUViewPreSalesList { get; set; }

    }
}
