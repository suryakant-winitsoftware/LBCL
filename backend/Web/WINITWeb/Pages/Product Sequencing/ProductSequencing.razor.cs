using Microsoft.AspNetCore.Components;

using Winit.Modules.Common.BL;

using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.SKU;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using WINITSharedObjects.Enums;
using System;

namespace WinIt.Pages.Product_Sequencing
{
    public partial class ProductSequencing
    {
        public int RowSequence = 0;
        string ActiveTab = "";
        private bool CheckAllRows = false;
        public int CurrentSeq;

        private Winit.UIComponents.Web.DialogBox.AddSKU AddSKURef;
        public bool IsAddSKUDialogOpen = false;
        private SelectionManager SelectedTab;
        //public List<ISelectionItem> TabSelectionItems = new List<ISelectionItem>
        //    {
        //  new SelectionItem{ Label="General", Code="General", UID="1"},
        // new SelectionItem{ Label="Purchase Order", Code="PurchaseOrder", UID="2"},
        // new SelectionItem{ Label="Load Template", Code="Template", UID="3"},
        // new SelectionItem{ Label="Cage", Code="Cage", UID="4"},
        // };
        private List<ISelectionItem> _tabSelectionItems;


        public List<ISelectionItem> TabSelectionItems
        {
            get
            {
                if (_tabSelectionItems == null)
                {
                    _tabSelectionItems = new List<ISelectionItem>
        {
            new SelectionItem { Label = @Localizer["general"], Code = "General", UID = "1" },
            new SelectionItem { Label = @Localizer["purchase_order"], Code = "PurchaseOrder", UID = "2" },
            new SelectionItem { Label = @Localizer["load_template"], Code = "Template", UID = "3" },
            new SelectionItem { Label = @Localizer["cage"], Code = "Cage", UID = "4" },
        };
                }
                return _tabSelectionItems;
            }
        }
        IDataService dataService = new DataServiceModel()
        {

            BreadcrumList = new List<IBreadCrum>()
            {

            }
        };
        public async Task SetHeaderName()
        {
            //_IDataService.BreadcrumList = new();
            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text =  @Localizer["home"], IsClickable = true, URL = "" });
            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = @Localizer["define_sku_sequence"], IsClickable = false });
            //_IDataService.HeaderText = @Localizer["define_sku_sequence"];
            //await CallbackService.InvokeAsync(_IDataService);
        }
        protected override async Task OnInitializedAsync()
        {
            dataService.HeaderText = "Define SKU Sequence";
            //dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text =  "home", IsClickable = true, URL = "" });
            dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = "Define SKU Sequence", IsClickable = false });

            _loadingService.ShowLoading("Loading...");
            LoadResources(null, _languageService.SelectedCulture);
            await Task.Run(async () =>
            {
                try
                {
                    FilterInitialized();
                    await _ProductSequencingViewModel.PopulateViewModel(null, null, Winit.Modules.Base.Model.CommonConstant.SeqType);
                    await SetHeaderName();
                    SetVariables();
                }
                catch (Exception ex) { }


                InvokeAsync(() =>
                {
                    _loadingService.HideLoading();

                    StateHasChanged(); // Ensure UI reflects changes
                });
            });
        }
        public void SetVariables()
        {
            ActiveTab = Winit.Modules.Base.Model.CommonConstant.SeqType;
            SelectedTab = new SelectionManager(TabSelectionItems, SelectionMode.Single);
            TabSelectionItems[0].IsSelected = ActiveTab == Winit.Modules.Base.Model.CommonConstant.SeqType;
        }

        private async Task DeleteSelectedSKUs()
        {

            try
            {
                if (_ProductSequencingViewModel.DisplaySkuSequencelist == null || _ProductSequencingViewModel.DisplaySkuSequencelist.Count <= 0)
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["add_atleast_one_sku"]);
                }
                else if (!_ProductSequencingViewModel.DisplaySkuSequencelist.Any(item => item.IsSelected))
                {
                    await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["select_atleast_one_sku"]);
                }
                else
                {
                    _ProductSequencingViewModel.MatchingNewExistingUIDs();
                    if (!_ProductSequencingViewModel.AllUIDs.Any(uid => _ProductSequencingViewModel.ExistingUIDs.Contains(uid)) || _ProductSequencingViewModel.ExistingUIDs == null || _ProductSequencingViewModel.ExistingUIDs.Count == 0)
                    {
                        bool result = await _alertService.ShowConfirmationReturnType("", @Localizer["are_you_sure_you_want_to_delete_selected_item"], @Localizer["yes"], @Localizer["no"]);
                        if (result)
                        {
                            _ProductSequencingViewModel.DisplaySkuSequencelist = _ProductSequencingViewModel.DisplaySkuSequencelist
                                  .Where(line => !line.IsSelected)
                                  .ToList();
                            _tost.Add(@Localizer["success"], @Localizer["template_successfully_deleted"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                    }
                    else
                    {
                        var skuSKUSequenceList = _ProductSequencingViewModel.PrepareListOfUidtForDelete();

                        bool result = await _alertService.ShowConfirmationReturnType("", @Localizer["are_you_sure_you_want_to_delete_selected_sku"], @Localizer["yes"], @Localizer["no"]);
                        if (result)
                        {
                            if (await _ProductSequencingViewModel.CreateUpdateDeleteSKUs(skuSKUSequenceList))
                            {
                                _tost.Add(@Localizer["skus"], @Localizer["skus_deleted_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);

                                await _ProductSequencingViewModel.PrepareDataForTable(ActiveTab);
                                CheckAllRows = false;
                            }
                            else { _tost.Add(@Localizer["skus"], @Localizer["skus_deleted_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error); }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }



        public async void OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                if (!selectionItem.IsSelected)
                {
                    SelectedTab.Select(selectionItem);
                    ActiveTab = selectionItem.Code;
                    _ProductSequencingViewModel.DisplaySkuSequencelist = null;
                    _ProductSequencingViewModel.FilterCriterias.Clear();
                    await _ProductSequencingViewModel.PrepareDataForTable(selectionItem.Code);
                    base.StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Exception", ex.Message.ToString());
            }
            finally
            {
                HideLoader();
            }
        }




        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }


        protected async Task SaveAndUpdateSKUs()
        {
            if (_ProductSequencingViewModel.DisplaySkuSequencelist == null || _ProductSequencingViewModel.DisplaySkuSequencelist.Count <= 0)
            {
                await _alertService.ShowErrorAlert(@Localizer["alert"], @Localizer["add_atleast_one_sku"]);
            }
            else
            {
                bool result = await _alertService.ShowConfirmationReturnType("", @Localizer["do_you_want_to_save_changes?"], @Localizer["yes"], @Localizer["no"]);
                if (result)
                {
                    try
                    {
                        if (await _ProductSequencingViewModel.PrepareDataForSaveUpdate())
                        {
                            _tost.Add(@Localizer["success"], @Localizer["skus_save/update_successfully"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                        else
                        {
                            _tost.Add(@Localizer["error"], @Localizer["skus_save/update_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
                        }
                    }

                    catch (Exception ex) { Console.WriteLine(ex.Message.ToString()); }
                }
            }

        }

        public async Task GetSelectedSKUs(List<ISelectionItem> selectionSKUs)
        {
            int count = 0;
            //try
            //{
            var maxSerialNo = _ProductSequencingViewModel.DisplaySkuSequencelist.Select(item => item.SerialNo)
                                .DefaultIfEmpty(0)
                                .Max();
            foreach (var item in selectionSKUs)
            {
                if (!_ProductSequencingViewModel.DisplaySkuSequencelist.Any(existingItem => existingItem.SKUCode == item.Code && existingItem.SeqType == ActiveTab))
                {
                    var SkuSequenceUI = new SkuSequenceUI
                    {
                        SKUCode = item.Code,
                        SKUName = item.Label,
                        SKUUID = item.UID,
                        SeqType = ActiveTab,
                        SerialNo = maxSerialNo + 1
                    };

                    maxSerialNo = maxSerialNo + 1;
                    _ProductSequencingViewModel.DisplaySkuSequencelist.Add(SkuSequenceUI);
                    count = count + 1;
                }
                else
                {
                    _tost.Add(@Localizer["route"], $"SKU with code {item.Code} Already Added for {ActiveTab}", Winit.UIComponents.SnackBar.Enum.Severity.Normal);

                }
            }



        }

        private void ToggleRow(SkuSequenceUI row)
        {

            row.IsSelected = !row.IsSelected;
        }
        int count = 0;
        protected void CurrentValue(SkuSequenceUI changedRow)
        {
            CurrentSeq = changedRow.SerialNo;
        }
        void HandleBlur(SkuSequenceUI changedRow)
        {
            //if (!StoredTab.Contains(ActiveTab))
            //{
            //    StoredTab.Add(ActiveTab);
            //}
            //  var tempDataList = _ProductSequencingViewModel.DisplaySkuSequencelist
            //.Where(r => r.SeqType == ActiveTab)
            //.OrderBy(r => r.SerialNo)
            //.ToList();
            //  _ProductSequencingViewModel.DisplaySkuSequencelist.RemoveAll(r => r.SeqType == ActiveTab);

            var seq = changedRow.SerialNo;
            int index = _ProductSequencingViewModel.DisplaySkuSequencelist.IndexOf(changedRow);
            _ProductSequencingViewModel.DisplaySkuSequencelist[index].SerialNo = changedRow.SerialNo;
            for (int i = 0; i < _ProductSequencingViewModel.DisplaySkuSequencelist.Count; i++)
            {
                if (CurrentSeq > seq)
                {


                    if (i != index && _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo == seq)
                    {
                        _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo = _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo + 1;

                    }
                }
                else
                {
                    if (i != index && _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo == seq)
                    {
                        _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo = _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo - 1;

                    }
                }
            }

            _ProductSequencingViewModel.DisplaySkuSequencelist = _ProductSequencingViewModel.DisplaySkuSequencelist.OrderBy(r => r.SerialNo).ToList();


            for (int i = 0; i < _ProductSequencingViewModel.DisplaySkuSequencelist.Count; i++)
            {
                _ProductSequencingViewModel.DisplaySkuSequencelist[i].SerialNo = i + 1;
            }
            //_ProductSequencingViewModel.DisplaySkuSequencelist.AddRange(_ProductSequencingViewModel.DisplaySkuSequencelist);
            base.StateHasChanged();
        }

        private void ToggleAllRows(ChangeEventArgs e)
        {
            CheckAllRows = !CheckAllRows;
            _ProductSequencingViewModel.DisplaySkuSequencelist.ForEach(item => item.IsSelected = CheckAllRows);
        }
        public void OpenAddProductDialog()
        {
            IsAddSKUDialogOpen = true;
            AddSKURef?.OpenAddProductDialog();
        }


        // Filter Logic

        private Winit.UIComponents.Web.Filter.Filter filterRef;
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public async void ShowFilter()
        {

            filterRef.ToggleFilter();
        }

        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
            {
                 new FilterModel { FilterType = FilterConst.TextBox, Label = @Localizer["sku_name/code"],ColumnName = ProductSequencingConst.Name},

            };
            showFilterComponent = true;
        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriterias)
        {
            try
            {
                ShowLoader();
                await _ProductSequencingViewModel.ApplyFilter(filterCriterias, ActiveTab);
            }
            catch (Exception)
            {

            }
            finally
            {
                HideLoader();
            }
        }

    }
}
