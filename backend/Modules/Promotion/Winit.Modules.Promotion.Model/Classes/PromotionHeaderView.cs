using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionHeaderView
    {
        public PromotionHeaderView()
        {
            this.promotionItemView = new List<PromotionItemView>();
        }
        public string SalesOrderUID { get; set; }
        public decimal TotalAmount { get; set; }
        public int LineCount { get; set; }
        public int BrandCount { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalDiscount { get; set; }
        public List<PromotionItemView> promotionItemView { get; set; }
        public List<InvoicePromotion> InvoicePromotions = new List<InvoicePromotion>();
        //public Dictionary<string, object> Attribute { get; set; } = new Dictionary<string, object>();
        //public List<SalesOrder.SalesOrderDiscount> LstSalesOrderDiscount { get; set; } = new List<SalesOrder.SalesOrderDiscount>();
        //public List<PromotionItemView> SalesOrderItemListFinal { get; set; } = new List<PromotionItemView>();
        //public List<Models.Domain.SalesOrder.SalesOrderItem> DisplaySalesOrderItemViewList { get; set; }
        //public string ApplicablePromotionListCommaSeparated { get; set; }
        //public List<string> ItemApplicablePrmotionUIDList { get; set; }
    }
}
