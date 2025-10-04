using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Web.SKU;

namespace Winit.UIComponents.Web.SalesManagement.PriceManagement
{
    public partial class AddProduct : ComponentBase
    {

        bool IsSearchable = true;
        object[] SearchedItems;
        [Parameter]
        public List<Winit.Modules.SKU.Model.Classes.SKU> SKUList { get; set; }
        [Parameter]
        public SelectionMode SelectionMode { get; set; }=SelectionMode.Multiple;

        [Parameter]
        public EventCallback<List<Winit.Modules.SKU.Model.Classes.SKU>> SendSKU { get; set; }


        protected void CheckORUncheckAll(bool isCheckAll)
        {

        }
        protected async Task CloseWithoutUpdating()
        {
            await SendSKU.InvokeAsync(null);
        }
        protected async Task AddSelectedProducts()
        {
            List<Winit.Modules.SKU.Model.Classes.SKU> sKU = SKUList.Where(p => p.IsSelected == true).ToList();
            await SendSKU.InvokeAsync(sKU);
            foreach(var sKUs in SKUList)
            {
                sKUs.IsSelected = false;
            }
        }

        protected  void SelectSKU(Winit.Modules.SKU.Model.Classes.SKU sKU)
        {
            if(SelectionMode == SelectionMode.Multiple)
            {
                sKU.IsSelected = !sKU.IsSelected;
            }
            else
            {
                foreach(var sKUs in SKUList)
                {
                    sKUs.IsSelected = false;
                }
                sKU.IsSelected=true;
            }
        }

       
    }
}
