using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Common.GridState;
using Winit.UIModels.Web.SKU;


using WinIt.Pages.Base;

namespace WinIt.Pages.SalesManagement.StandardPriceList
{
    public partial class MaintainStandardPriceList : BaseComponentBase
    {

        public List<DataGridColumn> ProductColumns { get; set; }

        [Inject] Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService { get; set; }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await GetSKUAttributeType();
            FilterInitialized();

            await SetHeaderName();
            await GenerateGridColumns();
            await _viewModel.PopulateViewModel();
        }

        public async Task SetHeaderName()
        {
            dataService.BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>();
            dataService.HeaderText = @Localizer["maintain_standard_price_list"];
            dataService.BreadcrumList.Add(new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_standard_price_list"] });

        }
        public async Task<ISKUAttributeLevel> GetSKUAttributeType()
        {
            _viewModel.sKUAttributeLevel = await _viewModel.GetAttributeType();

            if (_viewModel.sKUAttributeLevel == null)
            {
                _viewModel.sKUAttributeLevel = new SKUAttributeLevel()
                {
                    SKUGroupTypes = new(),
                    SKUGroups = new(),
                };
            }
            return _viewModel.sKUAttributeLevel;
        }
        private Winit.UIComponents.Web.Filter.Filter filterRef;
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
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label ="SKU Code",ColumnName = nameof(ISKUPrice.SKUCode), IsForSearch=true, PlaceHolder="Search By SKU Code", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label ="SKU Name",ColumnName = nameof(ISKUPrice.SKUName), IsForSearch=true, PlaceHolder="Search By SKU Name", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label ="Valid From",ColumnName = nameof(ISKUPrice.ValidFrom) },
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label ="Valid Upto",ColumnName = nameof(ISKUPrice.ValidUpto)},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=_viewModel.sKUAttributeLevel.SKUGroupTypes,OnDropDownSelect = OnAttributeTypeSelect, Label =@Localizer["attribute_type"] , ColumnName="AttributeType",HasChildDependency=true},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=_viewModel.AttributeNameSelectionItems, Label =@Localizer["attribute_name"] , ColumnName="AttributeValue",IsDependent=true,SelectionMode=SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,DropDownValues=_viewModel.ProductDivisionSelectionItems, Label = "Division", ColumnName="DivisionUID", SelectionMode = SelectionMode.Multiple},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.CheckBox, Label = "Show Inactive", ColumnName="IsActive"},
            };
        }
        private async Task OnAttributeTypeSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                await _viewModel.OnAttributeTypeSelect(dropDownEvent.SelectionItems.First().Code);
            }
        }
        private async Task GenerateGridColumns()
        {
            ProductColumns = new List<DataGridColumn>

        {
            new DataGridColumn { Header = @Localizer["sku_code"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).SKUCode, IsSortable = true, SortField = "SKUCode" },
            new DataGridColumn { Header = @Localizer["sku_name"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).SKUName, IsSortable = true, SortField = "SKUName" },
            new DataGridColumn { Header = @Localizer["buy_price"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).Price.ToString("0.00"), IsSortable = true, SortField = "Price" },
            new DataGridColumn { Header = @Localizer["wholesale_price"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).DefaultWSPrice.ToString("0.00") },
            new DataGridColumn { Header = @Localizer["retail_price"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).DefaultRetPrice.ToString("0.00") },
            new DataGridColumn { Header = @Localizer["uom"], GetValue = s => ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).UOM ,IsSortable=true,SortField="UOM"},
            new DataGridColumn { Header = @Localizer["effective_from"], GetValue = s =>Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat( ((Winit.Modules.SKU.Model.Classes.SKUPrice)s).ValidFrom ),IsSortable=true,SortField="ValidFrom"},
            new DataGridColumn { Header = @Localizer["effective_upto"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((Winit.Modules.SKU.Model.Classes.SKUPrice)s).ValidUpto),IsSortable=true,SortField="ValidUpto" },

        };
        }

    }
}
