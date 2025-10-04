using Microsoft.AspNetCore.Components;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.UIModels.Common.Filter;
using WinIt.Pages.Base;

namespace WinIt.Pages.SKUClassGroup
{
    public partial class SKUClassGroup : BaseComponentBase
    {
        public bool IsInitialized { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsMappingComponentOpen { get; set; }
        public required string ClassNameFilter { get; set; }
        public required string DistributionChannelFilter { get; set; }
        public required string NameFilter { get; set; }
        public required List<DataGridColumn> DataGridColumns { get; set; }
        private string? AllowedSKUUID { get; set; }
        private Winit.UIComponents.Web.Filter.Filter? FilterRef;
        public List<FilterModel> FilterColumns = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                LoadResources(null, _languageService.SelectedCulture);
                PrepareFilter();
              
                await GenerateTaxSKuMapGridColumns();
                await _SKUClassGroupViewModel.PopulateViewModel();
                IsInitialized = true;
                await StateChageHandler();
                HideLoader();
            }
            catch (Exception)
            {
                HideLoader();
                throw;
            }
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("AllowedSKU", ref FilterColumns, out PageState pageState);
            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("AllowedSKU");
        }
      
        public Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService =
            new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
            {
                BreadcrumList =
                [
                    new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                    {
                        SlNo = 1,
                        Text = "Manage Selling SKU",
                        IsClickable = true,
                        URL = "AllowedSKU"
                    },
                ],
                HeaderText = "MANAGE SELLING SKU"
            };

        private async Task GenerateTaxSKuMapGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new() { Header = @Localizer["name"], GetValue = s => ((ISKUClassGroup)s).Name },
                new()
                {
                    Header = @Localizer["startdate"],
                    GetValue = s => CommonFunctions.GetDateTimeInFormat(((ISKUClassGroup)s).FromDate)
                },
                new()
                {
                    Header = @Localizer["enddate"],
                    GetValue = s => CommonFunctions.GetDateTimeInFormat(((ISKUClassGroup)s).ToDate)
                },
                new()
                {
                    Header = @Localizer["actions"],
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new()
                        {
                            Text = @Localizer["mapping"], IsVisible = false, ConditionalVisibility = s => false,
                            Action = item => OnMappingBtnClick((ISKUClassGroup)item)
                        },
                        new()
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/edit.png",
                            Action = item => OnItemsBtnClick((ISKUClassGroup)item)
                        },
                        new()
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/delete.png", IsVisible = true, ConditionalVisibility = s => true,
                            Action = async item => await OnDeleteClick((ISKUClassGroup)item)
                        }
                    }
                }
            };
            await Task.CompletedTask;
        }

        public void OnMappingBtnClick(ISKUClassGroup sKUClassGroup)
        {
            AllowedSKUUID = sKUClassGroup.UID;
            IsMappingComponentOpen = true;
            StateHasChanged();
        }

        public void OnItemsBtnClick(ISKUClassGroup sKUClassGroup)
        {
            _navigationManager.NavigateTo(@$"SkuClassificationItemsMap?SKuClassGroupUID={sKUClassGroup.UID}");
        }

        public async Task OnDeleteClick(ISKUClassGroup sKUClassGroup)
        {
            if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"],
                    @Localizer["are_you_sure_you_want_to_delete_this_item_?"], Localizer["yes"], Localizer["no"]))
            {
                if (await _SKUClassGroupViewModel.OnSKUClassGroupDeleteClick(sKUClassGroup))
                {
                    _SKUClassGroupViewModel.SKUClassGroupsList.Remove(sKUClassGroup);
                    ShowSuccessSnackBar("", "Deleted successfully");
                    StateHasChanged();
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"], @Localizer["sku_class_group_delete_failed"]);
                }
            }
        }

        private async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            ShowLoader();
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            await _SKUClassGroupViewModel.ApplyFilter(keyValuePairs);
            HideLoader();
        }

        private void OnMappingSaveClick(List<IMappingItemView> mappingItemViews)
        {
            IsMappingComponentOpen = false;
        }

        private void PrepareFilter()
        {
            FilterColumns.AddRange(new List<FilterModel>
            {
                new()
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["name"],
                    ColumnName = nameof(ISKUClassGroup.Name)
                },
                new()
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["startdate"],
                    ColumnName = nameof(ISKUClassGroup.FromDate)
                },
                new()
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["enddate"],
                    ColumnName = nameof(ISKUClassGroup.ToDate)
                },
            });
        }
    }
}