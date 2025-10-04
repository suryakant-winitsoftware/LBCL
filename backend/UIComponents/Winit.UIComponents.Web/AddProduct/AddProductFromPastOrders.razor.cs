using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.DialogBoxes;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Common.LanguageResources.Mobile;

namespace Winit.UIComponents.Web.AddProduct
{
    public partial class AddProductFromPastOrders
    {
        List<Winit.UIModels.Web.Schema.ProductByInvoiceOrModel> productByInvoiceOrModels = new();
        public bool SelectByInvoice { get; set; } = true;
        public bool IsShowPopUpProducts { get; set; }
        public bool IsShowPopUpInvoices { get; set; }

        [Parameter]
        public EventCallback CloseDialog { get; set; }

        [Parameter]
        public EventCallback<List<Winit.UIModels.Web.Schema.ProductByInvoiceOrModel>> SelectedProducts { get; set; }
        protected override void OnInitialized()
        {
            IsShowPopUpProducts = true;
            for (int i = 0; i < 5; i++)
            {
                productByInvoiceOrModels.Add(new() { InvoiceNumber = "INVNO" + i, Date = DateTime.Now, ModelName = "MDLN" + i, ModelNumber = "MD" + i + i, TotalQty = i });
            }
            base.OnInitialized();
        }




        protected void CloseDialogMT()
        {
            CloseDialog.InvokeAsync();
        }
        protected void SearchASelectedProduct(string productName)
        {
            
        }
        protected void AddSelectedProducts()
        {
            if (SelectByInvoice)
            {
                SelectedProducts.InvokeAsync(productByInvoiceOrModels);
            }
            else if (IsShowPopUpInvoices)
            {
                SelectedProducts.InvokeAsync(productByInvoiceOrModels);
            }
            else
            {
                IsShowPopUpProducts = false;
                IsShowPopUpInvoices = true;
            }
        }
        protected void SelectSingleItemItem(Winit.UIModels.Web.Schema.ProductByInvoiceOrModel productByInvoiceOrModel)
        {
            foreach (var item in productByInvoiceOrModels)
            {
                item.IsSelected = false;
            }
            productByInvoiceOrModel.IsSelected = true;
        }
    }
}
