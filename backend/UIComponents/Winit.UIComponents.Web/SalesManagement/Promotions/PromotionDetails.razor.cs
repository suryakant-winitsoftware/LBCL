using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Common.Model.Classes;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.SKU.Model.Classes;
using Winit.UIModels.Common;

namespace Winit.UIComponents.Web.SalesManagement.Promotions
{
    public partial class PromotionDetails : ComponentBase
    {
        [Parameter]
        public List<ListItem> ListItems { get; set; }

        [Parameter]
        public string PromotionTypeCode { get; set; }

        [Parameter]
        public string PromotionUID { get; set; }

        [Parameter]
        public Winit.Modules.Promotion.Model.Classes.PromotionView PromotionView { get; set; }

        [Parameter]
        public Winit.UIComponents.Web.SalesManagement.Promotions.PromotionItems? PromotionItemsInstance { get; set; }

        [Parameter]
        public EventCallback<string> OnPromotionFormatChanged{ get; set; }

        protected bool IsLoad { get; set; }

        List<ListItem> listItems { get; set; }
        public List<ListItem> PROMOTION_TYPE { get; set; } = new();
        public List<ListItem> PROMOTION_CATEGORY { get; set; } = new();
        public List<ListItem> PROMO_INSTANT_FORMATS { get; set; } = new();
        PromotionItems promotion;
        protected override void OnInitialized()
        {

        }
        protected override async Task OnInitializedAsync()
        {


            if (PromotionView == null)
            {
                PromotionView = new();
            }
            promotion = new();
            if (ListItems != null && PromotionTypeCode != null)
            {
                SetDropDownLists(ListItems);
            }
        }

        protected void SetDropDownLists(List<ListItem> lists)
        {
            foreach (ListItem item in lists)
            {
                if (item.ListHeaderUID.Equals("PROMOTION CATEGORY"))
                {
                    PROMOTION_CATEGORY.Add(item);
                }
                if (item.ListHeaderUID.Equals("PROMO_INSTANT_FORMATS"))
                {
                    PROMO_INSTANT_FORMATS.Add(item);
                }
                if (item.ListHeaderUID.Equals(PromotionTypeCode))
                {
                    PROMOTION_TYPE.Add(item);
                }
            }
        }
        private void call()
        {
            promotion.GetRefresh("Something");
        }
        private async void OnPromotionchange(ChangeEventArgs e)
        {
            PromotionView.PromoFormat = e.Value.ToString();
            await OnPromotionFormatChanged.InvokeAsync(PromotionView.PromoFormat);
            //PromotionItemsInstance = PromotionItemsInstance == null ? new() : PromotionItemsInstance;
            //PromotionItemsInstance.SetPromotionItemSetup(PromotionView.PromoFormat);
        }

        
        public Winit.Modules.Promotion.Model.Classes.PromotionView GetPromotionDetailsFromComponent(string UID)
        {
            PromotionView.UID = UID;
            PromotionView.CreatedBy = _iAppUser.Emp.CreatedBy;
            PromotionView.ModifiedBy = _iAppUser.Emp.ModifiedBy;
            PromotionView.OrgUID = _iAppUser.Emp.CreatedBy;
            PromotionView.CreatedTime = DateTime.Now;
            PromotionView.ModifiedTime = DateTime.Now;
            PromotionView.ServerAddTime = DateTime.Now;
            PromotionView.ServerModifiedTime = DateTime.Now;
            PromotionView.UID = UID;


            return PromotionView;
        }
        public Validation IsPromotionDetailsValidated()
        {
            return new Validation(true);
        }

    }
}
