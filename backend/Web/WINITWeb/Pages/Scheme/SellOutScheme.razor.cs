using Microsoft.AspNetCore.Components;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Web.AddProduct;

namespace WinIt.Pages.Scheme
{
    public partial class SellOutScheme
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            HeaderText = "Sell Out Scheme",
            BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
             {
                 new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Manage Scheme",URL="ManageScheme",IsClickable=true},
                 new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Add Scheme",URL="AddScheme",IsClickable=true},
                  new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Sell Out Scheme" },
             }
        };

        List<ISelectionItem> selectionItems = new List<ISelectionItem>()
        {
            new SelectionItem(){UID="SelloutLiquidation" ,Label="Sellout Liquidation"},
            new SelectionItem(){UID="SelloutrealSecondaryScheme" ,Label="Sellout Real Secondary Scheme"},
        };
        string scheme = "SelloutLiquidation";
        protected void OnSelectionChange(DropDownEvent e)
        {
            if (e != null)
            {
                if (e.SelectionItems != null && e.SelectionItems.Count > 0)
                {
                    scheme=e.SelectionItems.First().UID;
                }
            }
            //scheme = e.Value.ToString();
        }
        private bool showModal = false;
        private string searchQuery = string.Empty;
        private List<Invoice> invoices = new List<Invoice>
    {
        new Invoice { Number = "3253535436", Date = new DateTime(2024, 6, 24), TotalQty = 4 },
        new Invoice { Number = "3253535436", Date = new DateTime(2024, 6, 20), TotalQty = 3 },
        new Invoice { Number = "3253535434", Date = new DateTime(2024, 6, 22), TotalQty = 16 },
        new Invoice { Number = "3253535436", Date = new DateTime(2024, 6, 14), TotalQty = 15 },
        new Invoice { Number = "3253535435", Date = new DateTime(2024, 6, 13), TotalQty = 12 },
        new Invoice { Number = "3253535437", Date = new DateTime(2024, 6, 12), TotalQty = 10 },
        new Invoice { Number = "3253535438", Date = new DateTime(2024, 6, 10), TotalQty = 20 },
        new Invoice { Number = "3253535439", Date = new DateTime(2024, 6, 8), TotalQty = 22 },
        new Invoice { Number = "3253535470", Date = new DateTime(2024, 6, 6), TotalQty = 4 },
        new Invoice { Number = "3253535471", Date = new DateTime(2024, 6, 5), TotalQty = 14 }
    };

        private void ToggleModal()
        {
            showModal = !showModal;
        }
        private void AddProductssFromPastOrders(List<Winit.UIModels.Web.Schema.ProductByInvoiceOrModel> productByInvoiceOrModels)
        {
            showModal = false;
        }

    }
     class Invoice
    {
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public int TotalQty { get; set; }
    }
}
