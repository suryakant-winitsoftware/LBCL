using Microsoft.AspNetCore.Components;
using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Promotion.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using WinIt.BreadCrum.Classes;
using WinIt.BreadCrum.Interfaces;

using WinIt.Pages.Base;
using WinIt.Pages.SalesManagement.Distributor;

namespace WinIt.Pages.SalesManagement.Promotions
{
    public partial class MaintainPromotions : BaseComponentBase
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService DataService { get; set; }
        protected override async Task OnInitializedAsync()
        {
            _loadingService.ShowLoading();
            LoadResources(null, _languageService.SelectedCulture);
            SetColumnHeaders();
            FilterInitialized();
            await _promotionBase.PageLoadFieldsOfMaintainPromotion();
            await StateChageHandler();
            await SetHeaderName();
            _promotionBase.IsLoad = true;
            _loadingService.HideLoading();

        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("MaintainPromotions", ref ColumnsForFilter, out PageState pageState);
            await OnFilterApply(_pageStateHandler._currentFilters);
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainPromotions");
        }
        async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            ShowLoader();
            _pageStateHandler._currentFilters = filterCriteria;
            await _promotionBase.OnFilterApply(filterCriteria);
            HideLoader();
        }
        async Task OnSort(SortCriteria sortCriteria)
        {
            ShowLoader();
            await _promotionBase.OnSort(sortCriteria);
            HideLoader();
        }
        public async Task SetHeaderName()
        {
            DataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
            {
                BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
                {
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text= @Localizer["maintain_promotions"], IsClickable = false, URL= "MaintainPromotions" }
                },
                HeaderText = @Localizer["maintain_promotions"]
            };
        }

        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public async void ShowFilter()
        {
            showFilterComponent = !showFilterComponent;
            filterRef.ToggleFilter();
        }


        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,ColumnName = nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Code), Label =@Localizer["promotion_code"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Name), Label = @Localizer["promotion_name"]},

                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,ColumnName=nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Type),Label =@Localizer["select_promotion_type"] ,
                                   DropDownValues= new() {
                                                            new SelectionItem() { UID=Winit.Shared.Models.Constants.Promotions.Line,Label=Winit.Shared.Models.Constants.Promotions.Line,Code=Winit.Shared.Models.Constants.Promotions.Line},
                                                            new SelectionItem() { UID = Winit.Shared.Models.Constants.Promotions.Assorted, Label = Winit.Shared.Models.Constants.Promotions.Assorted, Code = Winit.Shared.Models.Constants.Promotions.Assorted },
                                                            new SelectionItem() { UID=Winit.Shared.Models.Constants.Promotions.Invoice,Label=Winit.Shared.Models.Constants.Promotions.Invoice,Code=Winit.Shared.Models.Constants.Promotions.Invoice}
                                                         }
                 },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName =nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidFrom) , Label =@Localizer["startdate"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, ColumnName =nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidUpto) , Label =@Localizer["enddate"] },
             };
        }

        public List<DataGridColumn> Columns { get; set; }
        protected void SetColumnHeaders()
        {
            Columns = new List<DataGridColumn>
            {
                 new DataGridColumn { Header = @Localizer["code"],GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).Code, IsSortable = true, SortField = nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Code) },
                 new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).Name, IsSortable = true, SortField =nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Name)  },
                 new DataGridColumn { Header = @Localizer["category"], GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).Category, IsSortable = false, SortField =nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Category)  },
                 new DataGridColumn { Header = @Localizer["type"], GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).Type, IsSortable = true, SortField = nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Type) },
                 new DataGridColumn { Header = @Localizer["promoformat"], GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).PromoFormatLabel, IsSortable = true, SortField =nameof(Winit.Modules.Promotion.Model.Classes.Promotion.PromoFormat) }  ,
                 new DataGridColumn { Header = @Localizer["priority"], GetValue = s => ((Winit.Modules.Promotion.Model.Classes.Promotion)s).Priority ,IsSortable=true,SortField=nameof(Winit.Modules.Promotion.Model.Classes.Promotion.Priority)  },
                 new DataGridColumn { Header = @Localizer["validfrom"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((Winit.Modules.Promotion.Model.Classes.Promotion)s).ValidFrom),IsSortable=true,SortField=nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidFrom) },
                 new DataGridColumn { Header = @Localizer["validupto"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((Winit.Modules.Promotion.Model.Classes.Promotion)s).ValidUpto) ,IsSortable=true,SortField=nameof(Winit.Modules.Promotion.Model.Classes.Promotion.ValidUpto)},

                 new DataGridColumn
                 {
                     Header = @Localizer["actions"],
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Image,
                             URL = "Images/edit.png",
                             Action = item => ViewPromotions((Winit.Modules.Promotion.Model.Classes.Promotion)item,PageType.View)
                         },
                         //new ButtonAction
                         //{
                         //    ButtonType=ButtonTypes.Image,
                         //    URL = "Images/delete.png",
                         //    Action = async item =>await Deletepromotion((Winit.Modules.Promotion.Model.Classes.Promotion)item)
                         //},
                     }
                 }
           };
        }
        public async Task Deletepromotion(Winit.Modules.Promotion.Model.Classes.Promotion promotion)
        {
            if (promotion != null)
            {
                if (promotion.ValidFrom?.Date <= DateTime.Now.Date)
                {
                    await _alertService.ShowErrorAlert(@Localizer["error"], @Localizer["you_can_not_delete_this_promotion_because_its_is_applied_some_where_!"], null, @Localizer["ok"]);
                }
            }
        }
        public void ViewPromotions(Winit.Modules.Promotion.Model.Classes.Promotion promotion, string pageType)
        {
            _navigationManager.NavigateTo($"create-promotion?UID={promotion.UID}&{PageType.Page}={pageType}");
        }
    }

}
