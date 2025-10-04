using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.util;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common;
using Winit.UIModels.Web.Promotions;
using Winit.UIModels.Web.SKU;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;

using WinIt.Pages.Base;

namespace WinIt.Pages.SalesManagement.Promotions
{
    public partial class CreatePromotion : BaseComponentBase
    {
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            dataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn
                {
                    Header = @Localizer["s_no"],
                    GetValue = s =>((IMappingItemView)s).SNO
                },
                new DataGridColumn
                {
                    Header = @Localizer["type"],
                    GetValue = s =>((IMappingItemView)s)?.Type
                },
                new DataGridColumn
                {
                    Header = @Localizer["value"],
                    GetValue = s =>((IMappingItemView)s)?.Value
                },

            };
            iCreatePromotionBVM.PopulateViewModel();

            await iCreatePromotionBVM.PopulateViewmodelAsync();
            await mappingViewModel.PopulateViewModel(linkedItemType: Winit.Shared.Models.Constants.Promotions.Promotion, linkedItemUID: iCreatePromotionBVM.PromotionUID);
            SetHeaderName();
            iCreatePromotionBVM.IsLoad = true;
            _loadingService.HideLoading();
        }

        public bool IsCustomerselected { get; set; }
      
        MappingComponent? mappingComponent { get; set; }
        List<IMappingItemView>? mappingItemViews { get; set; }
        List<DataGridColumn>? dataGridColumns { get; set; }

        [CascadingParameter]
        public EventCallback<WinIt.BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        //public async Task SetHeaderName()
        //{
        //    try
        //    {
        //        _IDataService.BreadcrumList = new List<IBreadCrum>();
        //        _IDataService.HeaderText = @Localizer["add/edit_promotions"] ;
        //        _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_promotions"], IsClickable = true, URL = "MaintainPromotions" });
        //        _IDataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = $"{@Localizer["add / edit"]}{iCreatePromotionBVM.PromotionView.PromoTitle} {@Localizer["promotions"]} ", IsClickable = false, URL = "" });
        //        await CallbackService.InvokeAsync(_IDataService);
        //    }
        //    catch (Exception ex) { }
        //}
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService { get; set; }
        private void SetHeaderName()
        {
            try
            {
                dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
                {
                    BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
               {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text= @Localizer["maintain_promotions"], IsClickable = true, URL= "MaintainPromotions" },
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 2, Text = $"{@Localizer["add/edit"]}{iCreatePromotionBVM.PromotionView.PromoTitle} {@Localizer["promotions"]} " }
                },
                    HeaderText = @Localizer["maintain_promotions"]
                };
            }
            catch
            {

            }
        }
        WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<Winit.Modules.SKU.Model.Interfaces.ISKUV1> AddProductDialogBox;
        void AddProductClick()
        {
            //iCreatePromotionBVM.GetSelectedItemForSlab(e, promoOrder);
            //iCreatePromotionBVM.AddProducts = true;
            action = iCreatePromotionBVM.GetSelectedItems;
            AddProductDialogBox.OnOpenClick();
        }
        void AddProductClickFreeSKU(IPromoOrderForSlabs promoOrder)
        {
            SelectedItems.Clear();
            SelectedItems.AddRange(iCreatePromotionBVM.SKUList.FindAll(p => iCreatePromotionBVM.PromoOrderForSlabsList.
            Any(q => q.FreeSkuUID == p.UID)));
            //SKUList.ForEach(p => p.IsSelected = iCreatePromotionBVM.PromoOrderForSlabsList.Any(q => q.FreeSkuUID == p.UID));
            action = s => iCreatePromotionBVM.GetSelectedItemForSlab(s, promoOrder);
            AddProductDialogBox.OnOpenClick();
        }
        Action<List<ISKUV1>> action;
        List<ISKUV1> SelectedItems = [];
        void AddSkuGroupTypeSKU(bool isGroupTypeSKU)
        {
            SelectedItems.Clear();
            SelectedItems.AddRange(iCreatePromotionBVM.SKUList.FindAll(p => isGroupTypeSKU ?
            iCreatePromotionBVM.PromotionItemsModelList.Any(q => q.GroupUID == p.UID) :
            iCreatePromotionBVM.SelectedSKU.Any(q => q.UID == p.UID)));

            //SKUList.ForEach(p => p.IsSelected = isGroupTypeSKU ?
            //iCreatePromotionBVM.PromotionItemsModelList.Any(q => q.GroupUID == p.UID) :
            //iCreatePromotionBVM.SelectedSKU.Any(q => q.UID == p.UID));

            action = iCreatePromotionBVM.GetSelectedItems;
            iCreatePromotionBVM.AddSKUButtonType(isGroupTypeSKU);
            AddProductDialogBox.OnOpenClick();
        }
        async Task Save()
        {
            bool isMappingSaved = await mappingViewModel.SaveMapping();
            var isPromotionSaved = await iCreatePromotionBVM.SaveAsync();
            if (isMappingSaved && isPromotionSaved.IsValidated)
            {
                _tost.Add("Success", "Saved Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                _navigationManager.NavigateTo("MaintainPromotions");
            }
            else
            {
                _tost.Add("Error", isPromotionSaved.ErrorMessage, Winit.UIComponents.SnackBar.Enum.Severity.Error);
            }
        }
    }
}
