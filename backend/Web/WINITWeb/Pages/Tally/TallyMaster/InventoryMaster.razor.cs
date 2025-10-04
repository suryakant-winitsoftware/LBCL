using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Tally.TallyMaster
{
    public partial class InventoryMaster : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public bool IsChannelPartnerSelected { get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; } = new List<DataGridColumn>();
        public List<DataGridColumn> DataGridColumnsForExcel { get; set; } = new List<DataGridColumn>();
        public bool IsItemSelectedToShow { get; set; } = false;
        List<FilterModel> ColumnsForFilter = [];
        public string SelectedDealer = null;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Inventory",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Inventory"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            tallyMasterViewModel.PageSize = 100;
            await tallyMasterViewModel.PopulateChannelPartners();
            SetFilters();
            IsInitialised = true;
            HideLoader();
        }
        public async Task OnChannelPartnerSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                SelectedDealer = selectedItem.UID;
                SelectedDealer = selectedItem.UID;
                tallyMasterViewModel.UID = selectedItem.UID;
                await tallyMasterViewModel.GetInventoryMasterGridDataByDist(selectedItem!.UID);
                GenerateGridColumns();
                GenerateGridColumnsForExcel();
                IsChannelPartnerSelected = true;
            }
            else
            {
                tallyMasterViewModel.UID = null;
                IsChannelPartnerSelected = false;
                ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
            }
        }

        private async Task PageIndexChanged(int pageNumber)
        {
            ShowLoader();
            await tallyMasterViewModel.PageIndexChanged(pageNumber, tallyMasterViewModel.UID, "Inventory");
            HideLoader();
        }

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Name", GetValue = item => ((ITallyInventoryMaster)item).Name ?? "N/A" ,IsSortable=true,SortField="Name"},
            //    new DataGridColumn { Header = "Code", GetValue = item => ((ITallyInventoryMaster)item).Code ?? "N/A"},
                new DataGridColumn { Header = "Units", GetValue = item => ((ITallyInventoryMaster)item).Units ?? "N/A",IsSortable=true,SortField="Units"},
             //   new DataGridColumn { Header = "Last Record Date", GetValue = item => ((ITallyInventoryMaster)item).LastRecordDate ?? "N/A"},
                new DataGridColumn { Header = "Opening Balance", GetValue = item => ((ITallyInventoryMaster)item).OpeningBalance ?? "N/A",IsSortable=true,SortField="OpeningBalance"},
                new DataGridColumn { Header = "Opening Rate", GetValue = item => ((ITallyInventoryMaster)item).OpeningRate ?? "N/A",IsSortable=true,SortField="OpeningRate"},
                new DataGridColumn { Header = "Stock Group", GetValue = item => ((ITallyInventoryMaster)item).StockGroup ?? "N/A",IsSortable=true,SortField="StockGroup"},
                new DataGridColumn { Header = "Parent", GetValue = item => ((ITallyInventoryMaster)item).Parent ?? "N/A",IsSortable=true,SortField="Parent"},
                new DataGridColumn { Header = "GST Details", GetValue = item => ((ITallyInventoryMaster)item).GstDetails ?? "N/A",IsSortable=true,SortField="GstDetails"},
            //    new DataGridColumn { Header = "Remote Alt Guid", GetValue = item => ((ITallyInventoryMaster)item).RemoteAltGuid ?? "N/A"},
              new DataGridColumn { Header = "Distributor Name", GetValue = item => ((ITallyInventoryMaster)item).DistributorCode ?? "N/A",IsSortable=true,SortField="DistributorCode"},
                new DataGridColumn { Header = "Action", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/view.png",
                            Action = item => OnViewClick((ITallyInventoryMaster)item)
                        },
                    }},
            };
        }

        private void GenerateGridColumnsForExcel()
        {
            DataGridColumnsForExcel = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Name", GetValue = item => ((ITallyInventoryMaster)item).Name ?? "N/A"},
                new DataGridColumn { Header = "Code", GetValue = item => ((ITallyInventoryMaster)item).Code ?? "N/A"},
                new DataGridColumn { Header = "Units", GetValue = item => ((ITallyInventoryMaster)item).Units ?? "N/A"},
                new DataGridColumn { Header = "Last Record Date", GetValue = item => ((ITallyInventoryMaster)item).LastRecordDate ?? "N/A"},
                new DataGridColumn { Header = "Opening Balance", GetValue = item => ((ITallyInventoryMaster)item).OpeningBalance ?? "N/A"},
                new DataGridColumn { Header = "Opening Rate", GetValue = item => ((ITallyInventoryMaster)item).OpeningRate ?? "N/A"},
                new DataGridColumn { Header = "Stock Group", GetValue = item => ((ITallyInventoryMaster)item).StockGroup ?? "N/A"},
                new DataGridColumn { Header = "Parent", GetValue = item => ((ITallyInventoryMaster)item).Parent ?? "N/A"},
                new DataGridColumn { Header = "GST Details", GetValue = item => ((ITallyInventoryMaster)item).GstDetails ?? "N/A"},
                new DataGridColumn { Header = "Remote Alt Guid", GetValue = item => ((ITallyInventoryMaster)item).RemoteAltGuid ?? "N/A"},
                new DataGridColumn { Header = "Distributor Name", GetValue = item => ((ITallyInventoryMaster)item).DistributorCode ?? "N/A"},
                
            };
        }
        private async Task OnViewClick(ITallyInventoryMaster item)
        {
            ShowLoader();
            await tallyMasterViewModel.GetInventoryMasterItemDetails(item.RemoteAltGuid);
            IsItemSelectedToShow = true;
            StateHasChanged();
            HideLoader();
        }
        private void OnBackButtonClick()
        {
            IsItemSelectedToShow = false;
        }
        protected void SetFilters()
        {
            ColumnsForFilter = new List<FilterModel>
             {
                new FilterModel
                {
                    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                    Label = "Name",
                    ColumnName = "Name"

                },
                //new FilterModel
                //{
                //    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                //    Label = "Ledger Name",
                //    ColumnName = "ledger_name"
                //},
                //new FilterModel
                //{
                //    FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                //    Label = "Parent Name",
                //    ColumnName = "parent_name"
                //}

             };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            if (SelectedDealer != null)
            {
                _loadingService.ShowLoading();
                List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Key == "Name")
                        {
                            filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));

                        }
                        //else if (keyValue.Key == "ledger_name")
                        //{
                        //    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                        //}
                        //else if (keyValue.Key == "parent_name")
                        //{
                        //    filterCriterias.Add(new FilterCriteria(@$"{keyValue.Key}", keyValue.Value, FilterType.Like));
                        //}

                    }
                }
                await tallyMasterViewModel.ApplyFilterForInventory(filterCriterias, SelectedDealer);
                StateHasChanged();
                _loadingService.HideLoading();
            }
            else
            {
                ShowErrorSnackBar("Info :", "Please select channel partner for provisioning");
            }

        }
        private async Task OnSortApply(SortCriteria sortCriteria)
        {
            ShowLoader();
            await tallyMasterViewModel.ApplySort(sortCriteria, tallyMasterViewModel.UID, "Inventory");
            StateHasChanged();
            HideLoader();
        }
        private async Task OnExcelDownloadClick()
        {
            try
            {
                tallyMasterViewModel.PageSize = 0;
                tallyMasterViewModel.PageNumber = 0;
                await tallyMasterViewModel.PopulateGridDataForEXCEL(tallyMasterViewModel.UID, "Inventory");
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
