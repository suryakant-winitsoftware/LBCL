using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Winit.Modules.Common.BL;
using Winit.Modules.ReturnOrder.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using WinIt.Pages.Base;
using Winit.Shared.Models.Enums;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.SKUClass.BL.UIClasses;

namespace WinIt.Pages.SKUClassGroupItems;

partial class SKUClassGroupItemsBasePageV1 : BaseComponentBase
{
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
                new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel()
                {
                    SlNo = 2,
                    Text = "Add / Edit Selling SKU",
                    IsClickable = false
                },
            ],
            HeaderText = "ADD / EDIT SELLING SKU"
        };

    private string SKUClassGroupUID { get; set; } = string.Empty;
    public List<DataGridColumn>? DataGridColumns { get; set; }
    public bool IsInitialized { get; set; }
    private Winit.UIComponents.Common.CustomControls.DropDown? DistributionDDL;
    private WinIt.Pages.DialogBoxes.AddProductDialogBoxV1<ISKUV1>? AddProductDialogBox;

    private List<ISKUV1> SKUs = new List<ISKUV1>();

    bool IsAddExcludedItem { get; set; }
    private List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>();
    private List<DataGridColumn> GridColumns = [];
    private ISelectionItem SelectedTab { get; set; }
    private SelectionManager TabSelectionManager => new SelectionManager(TabSelectionItems, SelectionMode.Single);
    private bool _showAllCustomers = false;
    protected override void OnInitialized()
    {
        ShowLoader();
        SKUClassGroupUID = _commonFunctions.GetParameterValueFromURL("SKuClassGroupUID");
        LoadResources(null, _languageService.SelectedCulture);
        GenerateTabSelectionItems();
        GenerateGridColumns();
        HideLoader();
    }
    protected async override Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            await _viewModel.PopulateViewModel(SKUClassGroupUID);
            await _viewModel.PopulateApplicableToCustomersAndSKU();

            OnTabSelect(TabSelectionItems.FirstOrDefault());
            await mappingViewModel.PopulateViewModel(
                linkedItemType: "AllowedSKU",
                linkedItemUID: _viewModel.SKUClassGroupMaster.SKUClassGroup!.UID);
            //CalculateItemsCountForAllTabs();
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            HideLoader();
        }

        StateHasChanged();
    }

    private void GenerateTabSelectionItems()
    {
        TabSelectionItems.Clear();
        TabSelectionItems.Add(new SelectionItemTab { UID = "1", Code = "1", Label = "All Active SKUs" });
        TabSelectionItems.Add(new SelectionItemTab { UID = "2", Code = "2", Label = "Selling SKUs" });
        TabSelectionItems.Add(new SelectionItemTab { UID = "3", Code = "3", Label = "Excluded SKUs" });
    }

    private void GenerateGridColumns()
    {
        GridColumns.Clear();
        GridColumns.AddRange(new[]
        {
            new DataGridColumn
            {
                Header = "Org Unit",
                GetValue = s =>
                    $"{((ISKUV1)s).L1}",
                SortField = "orguid"
            },
            new DataGridColumn
            {
                Header = "Division",
                GetValue = s =>
                    $"{((ISKUV1)s).L2}",
                SortField = "SupplierOrgUID"
            },
            new DataGridColumn
            {
                Header = "Product Category",
                GetValue = s =>
                    $"{((ISKUV1)s).L3}",
                SortField = "ProductCategory"
            },
            new DataGridColumn
            {
                Header = "Type",
                GetValue = s =>
                    $"{((ISKUV1)s).L4}",
                SortField = "Type"
            },
            new DataGridColumn
            {
                Header = "Star Rating",
                GetValue = s =>
                    $"{((ISKUV1)s).L5}",
                SortField = "StarRating"
            },
            new DataGridColumn
            {
                Header = "Series",
                GetValue = s =>
                    $"{((ISKUV1)s).L6}",
                SortField = "Series"
            },
            new DataGridColumn
            {
                Header = "Model Number",
                GetValue = s =>
                    $"{((ISKUV1)s).Code}",
                SortField = "ModelNumber"
            },
            new DataGridColumn
            {
                Header = "Model Name",
                GetValue = s =>
                    $"{((ISKUV1)s).Name}",
                SortField = "ModelName"
            },
            //new DataGridColumn
            //{
            //    Header = "Action",
            //    IsButtonColumn = true,

            //    ButtonActions = new List<ButtonAction>()
            //        {
            //            new ButtonAction()
            //            {
            //                     Text="Delete",
            //                    Action =async o =>
            //                    {
            //                        if(await _alertService.ShowConfirmationReturnType("Alert","are you sure you want to delete?"))
            //                        {

            //                        _viewModel.GridSKUs.Remove((o as ISKUV1));
            //                        CalculateItemsCountForAllTabs();
            //                        StateHasChanged();
            //                        }
            //                    },
            //                    ConditionalVisibility=s=>SelectedTab.UID=="1"&&SelectedTab.UID!="2"&&SelectedTab.UID!="3"?true:false,
            //                    IsVisible=SelectedTab.UID=="1"&&SelectedTab.UID!="2"&&SelectedTab.UID!="3"?true:false,
            //            },
            //        new ButtonAction()
            //            {
            //                     Text="Add To Excluded SKU",
            //                    Action = o =>
            //                    {
            //                        _viewModel.SkuExcludeList.Add((o as ISKUV1).UID);
            //                        CalculateItemsCountForAllTabs();
            //                        StateHasChanged();
            //                    },
            //                    ConditionalVisibility=s=>SelectedTab.UID=="2"?true:false,
            //                    IsVisible=SelectedTab.UID=="2"?true:false,
            //            },
            //        new ButtonAction()
            //            {
            //                     Text="Remove from Excluded SKU",
            //                    Action = o =>
            //                    {
            //                        _viewModel.SkuExcludeList.Remove((o as ISKUV1).UID);
            //                        CalculateItemsCountForAllTabs();
            //                        StateHasChanged();
            //                    },
            //                    ConditionalVisibility=s=>SelectedTab.UID=="3"?true:false,
            //                    IsVisible=SelectedTab.UID=="3"?true:false,
            //            },
            //        }
            //},
        });
    }

    private async Task OnOrgSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem selectedOrg = dropDownEvent.SelectionItems.First();
            await _viewModel.OnOrgSelect(selectedOrg);
            DistributionDDL?.GetLoad();
        }
    }

    private async Task OnDistributionChannelSelect(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            ISelectionItem selectedDistributionChannel = dropDownEvent.SelectionItems.First();
            _viewModel.SKUClassGroupMaster.SKUClassGroup!.DistributionChannelUID = selectedDistributionChannel.UID;
        }

        await Task.CompletedTask;
    }

    private async Task OnBackClick()
    {
        if (await _alertService.ShowConfirmationReturnType(@Localizer["alert"],
                @Localizer["are_you_sure_you_want_to_go_back_?"]))
        {
            _navigationManager.NavigateTo("AllowedSKU");
        }
    }

    private async Task OnSaveClick()
    {
        ShowLoader();
        if (await Validate())
        {
            ApiResponse<string> resp = await _viewModel.OnSaveClick();
            if (resp != null)
            {
                if (resp.IsSuccess)
                {
                    ShowSuccessSnackBar(@Localizer["success"], @Localizer["saved_successfully"]);
                    _navigationManager.NavigateTo("AllowedSKU");
                }
                else
                {
                    ShowErrorSnackBar(@Localizer["error"],
                        resp.StatusCode == 409 ? "Name already exist" : $"[{resp.StatusCode}] {resp.ErrorMessage}");
                }
            }
        }

        HideLoader();
    }

    private async Task<bool> Validate()
    {
        if ( //string.IsNullOrEmpty(_viewModel.SKUClassGroupMaster.SKUClassGroup!.OrgUID) ||
            string.IsNullOrWhiteSpace(_viewModel.SKUClassGroupMaster.SKUClassGroup!.Name) ||
            string.IsNullOrEmpty(_viewModel.SKUClassGroupMaster.SKUClassGroup!.Name) //||
                                                                                     //string.IsNullOrEmpty(_viewModel.SKUClassGroupMaster.SKUClassGroup!.Description)
           )
        {
            ShowErrorSnackBar("", "Name Shouldn't be empty");
            return false;
        }
        else if (!_viewModel.SelectedBC.Any() && !_viewModel.SelectedCP.Any() && !_viewModel.SelectedBranches.Any())
        {
            ShowErrorSnackBar("", "Select Broad Classification or Branch or Channel Partner");
            return false;
        }
        else if (!_viewModel.GridSKUs!.Any())
        {
            ShowErrorSnackBar("", "SKU's Shouldn't be empty");
            return false;
        }
        else if (_viewModel.SKUClassGroupMaster.SKUClassGroup.ToDate <
                 _viewModel.SKUClassGroupMaster.SKUClassGroup.FromDate)
        {
            ShowErrorSnackBar("", "EndDate should not be less than start date");
            return false;
        }

        return true;
    }

    private async Task OnAddSKUClick(List<ISKUV1> skus)
    {
        //await _viewModel.AddSKUsToGrid(skus);
        // _viewModel.AddProductsToGrid(skus);
        skus.ForEach(p =>
        {
            if (!_viewModel.GridSKUs.Contains(p))
            {
                _viewModel.GridSKUs.Add(p);
                if (IsAddExcludedItem)
                {
                    _viewModel.SkuExcludeList.Add(p.UID);
                }
            }
        });
        //_viewModel.GridSKUs.AddRange(skus.Except(_viewModel.GridSKUs));

        //if (IsAddExcludedItem)
        //{
        //    IsAddExcludedItem = false;
        //}

        CalculateItemsCountForAllTabs();
        StateHasChanged();
    }

    private void AddSKU(bool isExcludedItem = false)
    {
        IsAddExcludedItem = isExcludedItem;
        AddProductDialogBox?.OnOpenClick();
    }

    private async Task PlantDDClicked(
        Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView sKUClassGroupItemView)
    {
        await _dropdownService.ShowDropDown(new Winit.UIComponents.Common.Services.DropDownOptions
        {
            DataSource = _viewModel.PlantSelectionItems,
            SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Single,
            OnSelect = async (eventArgs) =>
            {
                // Your asynchronous handling logic goes here
                await OnPlantDDSelect(sKUClassGroupItemView, eventArgs);
            },
        });
    }

    private async Task OnPlantDDSelect(
        Winit.Modules.SKUClass.Model.UIInterfaces.ISKUClassGroupItemView sKUClassGroupItemView,
        DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null && dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
        {
            sKUClassGroupItemView.SupplierOrgUID = dropDownEvent.SelectionItems.First().UID;
            sKUClassGroupItemView.PlantName = dropDownEvent.SelectionItems.First().Label;
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnTabSelect(ISelectionItem selectionItem)
    {
        TabSelectionManager.Select(selectionItem);
        SelectedTab = selectionItem;
        DataGridColumn actionColumn = GridColumns.Find(e => e.Header == "Action");
        if (SelectedTab != null && SelectedTab.UID == "1")
        {
            if (actionColumn is null)
            {
                actionColumn = new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,

                    ButtonActions = new List<ButtonAction>()
                    {
                        new ButtonAction()
                        {
                        }
                    }
                };
                GridColumns.Add(actionColumn);
            }

            actionColumn.ButtonActions.FirstOrDefault().Action = async o =>
            {
                if (await _alertService.ShowConfirmationReturnType("Alert", "are you sure you want to delete?"))
                {
                    _viewModel.GridSKUs.Remove((o as ISKUV1));
                    CalculateItemsCountForAllTabs();
                    StateHasChanged();
                }
            };
            actionColumn.ButtonActions.FirstOrDefault().Text = "Delete";
        }
        else if (SelectedTab != null && SelectedTab.UID == "2")
        {
            if (actionColumn is null)
            {
                actionColumn = new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,

                    ButtonActions = new List<ButtonAction>()
                    {
                        new ButtonAction()
                        {
                        }
                    }
                };
                GridColumns.Add(actionColumn);
            }

            actionColumn.ButtonActions.FirstOrDefault().Action = o =>
            {
                _viewModel.SkuExcludeList.Add((o as ISKUV1).UID);
                CalculateItemsCountForAllTabs();
                StateHasChanged();
            };
            actionColumn.ButtonActions.FirstOrDefault().Text = "Add To Excluded SKU";
        }
        else
        {
            if (actionColumn is null)
            {
                actionColumn = new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,

                    ButtonActions = new List<ButtonAction>()
                    {
                        new ButtonAction()
                        {
                        }
                    }
                };
                GridColumns.Add(actionColumn);
            }

            actionColumn.ButtonActions.FirstOrDefault().Action = o =>
            {
                _viewModel.SkuExcludeList.Remove((o as ISKUV1).UID);
                CalculateItemsCountForAllTabs();
                StateHasChanged();
            };
            actionColumn.ButtonActions.FirstOrDefault().Text = "Remove from Excluded SKU";
        }

        CalculateItemsCountForAllTabs();
        StateHasChanged();
    }

    private void CalculateItemsCountForAllTabs()
    {
        TabSelectionItems.ForEach(e =>
        {
            SelectionItemTab tab = e as SelectionItemTab;
            switch (tab.UID)
            {
                case "1":
                    tab.Count = _viewModel.GridSKUs.Count;
                    break;
                case "2":
                    tab.Count = _viewModel.GridSKUs.Count(e => !_viewModel.SkuExcludeList.Contains(e.UID));
                    break;
                case "3":
                    tab.Count = _viewModel.GridSKUs.Count(e => _viewModel.SkuExcludeList.Contains(e.UID));
                    break;
            }
        });
    }

    private async Task<List<ISelectionItem>> OnDropdownValueSelectSKUAtrributes(ISelectionItem selectionItem)
    {
        try
        {
            return await _viewModel.OnSKuAttributeDropdownValueSelect(selectionItem.UID);
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private bool OnItemSelect(ISKUV1 item)
    {
        return true;
    }

    private void OnBroadClassificationSelected(DropDownEvent dropDownEvent)
    {
        _viewModel.OnBroadClassificationSelected(dropDownEvent);
    }

    private void OnChannelpartnerSelected(DropDownEvent dropDownEvent)
    {
        if (dropDownEvent != null)
        {
            _viewModel.OnChannelpartnerSelected(dropDownEvent);
        }
    }

    List<IStore> Stores = [];

    private async Task OnShowAllClick()
    {
        Stores.Clear();
        var strs = await ((SKUClassGroupItemsWebViewModelV1)_viewModel).GetApplicableToCustomers();
        if (strs == null)
        {
            _tost.Add("", "please select any one in applicable to customers");
            return;
        }

        Stores.AddRange(strs);
        _showAllCustomers = true;

        StateHasChanged();
    }

    private void OnAddProductClick(List<ISKUV1> products)
    {
        if (products == null || !products.Any()) return;
        _viewModel.AddProductsToGrid(products);
        CalculateItemsCountForAllTabs();
        StateHasChanged();
    }
}