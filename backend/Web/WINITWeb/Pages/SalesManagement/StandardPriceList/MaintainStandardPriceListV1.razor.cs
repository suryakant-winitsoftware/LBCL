using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;


namespace WinIt.Pages.SalesManagement.StandardPriceList
{
    public partial class MaintainStandardPriceListV1
    {
        public List<DataGridColumn> ProductColumns { get; set; }

        public Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
        {
            BreadcrumList =
            [
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "Maintain Standared PriceList", IsClickable = false },
            ],
            HeaderText = "Maintain Standared PriceList"
        };

        protected override void OnInitialized()
        {
            LoadResources(null, _languageService.SelectedCulture);
                base.OnInitialized();
        }
        protected override async Task OnInitializedAsync()
        {
            await GetSKUAttributeType();
            FilterInitialized();
            await _viewModel.PopulateViewModel();
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
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["sku_code/name"],ColumnName = "skucodeandname", IsForSearch=true, PlaceHolder="Search By SKU Code / Name", Width=1000},
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
    }
}
