using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.Location.Model.Classes;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;
using WINITSharedObjects.Enums;

namespace WinIt.Pages.Branch_Mapping
{
    public partial class MaintainBranch : BaseComponentBase
    {
        //public string SalesOfficeBranchUid { get; set; }
        //public string SalesOfficeName { get; set; }
        //public string SalesOfficeCode { get; set; }
        public ISalesOffice SalesOffice { get; set; } = new SalesOffice();
        public bool IsInitailised { get; set; } = false;
        public bool AddEditSalesOffice { get; set; } = false;
        public List<FilterModel> FilterColumns = new List<FilterModel>()
        {
            new FilterModel(){Label="Branch Code",ColumnName=nameof(IBranch.Code),FilterType=FilterConst.TextBox},
            new FilterModel(){Label="Branch Name",ColumnName=nameof(IBranch.Name),FilterType=FilterConst.TextBox},
             new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                     Label = "status",DropDownValues=new List<ISelectionItem>
                            {
                                new SelectionItem{UID="1",Code="1",Label="Active"},
                                new SelectionItem{UID="0",Code="0",Label="Inactive"},
                            },
                     ColumnName=nameof(IBroadClassificationHeader.IsActive),SelectionMode=SelectionMode.Single},
            };

        public List<DataGridColumn> DataGridColumns { get; set; }
        public List<DataGridColumn> DataGridSalesOfficeColumns { get; set; }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Branch",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Maintain Branch"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await maintainBranchMappingViewModel.PopulateViewModel();
            await GenerateGridColumns();
            IsInitailised = true;
            await StateChageHandler();
            HideLoader();

        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("MaintainBranch", ref FilterColumns, out PageState pageState);

            ///only work with filters
            await OnFilterApply(_pageStateHandler._currentFilters);

        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("MaintainBranch");
        }
        public async Task OnFilterApply(IDictionary<string, string> keyValuePairs)
        {
            ShowLoader();
            _pageStateHandler._currentFilters = (Dictionary<string, string>)keyValuePairs;
            await maintainBranchMappingViewModel.OnFilterApply(keyValuePairs);
            HideLoader();
            StateHasChanged();
        }
        private async Task AddNewBranchDetails()
        {
            _navigationManager.NavigateTo($"EditMainTainBranch");
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn
                {
                    Header = "Code",
                    GetValue = item => ((IBranch)item).Code ?? "N/A",IsSortable=true,SortField=nameof(IBranch.Code)
                },
                new DataGridColumn
                {
                    Header = "Name",
                    GetValue = item => ((IBranch)item).Name ?? "N/A",IsSortable=true,SortField=nameof(IBranch.Name)
                },
                new DataGridColumn
                {
                    Header = "No Of States",
                    GetValue = item => ((IBranch)item).Level1Count.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "No Of Cities",
                    GetValue = item => ((IBranch)item).Level2Count.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "No Of Localities",
                    GetValue = item => ((IBranch)item).Level3Count.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "Is Active",
                    GetValue = item => ((IBranch)item).IsActive.ToString() ?? "N/A"
                },
                new DataGridColumn
                {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = item => OnEditClick((IBranch)item)
                    },
                    new ButtonAction
                    {
                        ButtonType  = ButtonTypes.Text,
                        Action = item => OnAddEditSalesOffice((IBranch)item),
                        Text = "Add Sales Office"
                    },
                }
            }
            };
        }
        private void OnEditClick(IBranch item)
        {
            var encodedUID = Uri.EscapeDataString(item.UID);
            _navigationManager.NavigateTo($"EditMaintainBranch?BranchMappingUID={encodedUID}");
        }
        private async void OnAddEditSalesOffice(IBranch item)
        {
            if (item.UID != null)
            {
                SalesOffice = new SalesOffice
                {
                    BranchUID = item.UID
                };
                await maintainBranchMappingViewModel.GetBranchMappingSalesOffices(item.UID);
                AddEditSalesOffice = true;
                GenerateGridColumnsForSalesOffice();
                StateHasChanged();
            }
        }
        public async Task OnSalesOfficeOrgSelect(DropDownEvent dropDownEvent)
        {
            if (dropDownEvent.SelectionItems.Any(item => item.IsSelected))
            {
                var selectedItem = dropDownEvent.SelectionItems.FirstOrDefault(item => item.IsSelected);
                SalesOffice.WareHouseUID = selectedItem.UID;    // + "["+selectedItem.Label +"]";
            }

        }
        private void GenerateGridColumnsForSalesOffice()
        {
            DataGridSalesOfficeColumns = new List<DataGridColumn>
            {
                new DataGridColumn
                {
                    Header = "Code",
                    GetValue = item => ((ISalesOffice)item).Code ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "Name",
                    GetValue = item => ((ISalesOffice)item).Name ?? "N/A"
                },
                new DataGridColumn
                {
                    Header = "Warehouse",
                    GetValue = item =>
                        {
                            var salesOffice = (ISalesOffice)item;
                         //   var warehousecode = maintainBranchMappingViewModel.SalesOfficeOrgType.FirstOrDefault(w => w.UID == salesOffice.WareHouseUID)?.Code?.ToString() ?? "N/A";
                            var warehouseLabel = maintainBranchMappingViewModel.SalesOfficeOrgType.FirstOrDefault(w => w.UID == salesOffice.WareHouseUID)?.Label ?? "N/A";
                            return /*"[" +warehousecode+"] "+ */ warehouseLabel;
                        }
                },
                new DataGridColumn
                {
                Header = "Action",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = item => OnDeleteSalesOfficeDetails((ISalesOffice)item)
                    },
                }
            }
            };
        }

        private async void OnDeleteSalesOfficeDetails(ISalesOffice item)
        {
            if (await maintainBranchMappingViewModel.DeleteSalesOfficeDetails(item))
            {
                await maintainBranchMappingViewModel.GetBranchMappingSalesOffices(SalesOffice.BranchUID);
                StateHasChanged();
                ShowSuccessSnackBar("Success", "Data Deleted Successfully");
            }
            else
            {
                ShowErrorSnackBar("Failed", "Data Deleted UnSuccessful");
            }
        }

        private async void AddSalesOfficeDetails()
        {
            if (ValidateSalesOfficeData())
            {
                await SaveFormsData();
                await maintainBranchMappingViewModel.GetBranchMappingSalesOffices(SalesOffice.BranchUID);
                SalesOffice = new SalesOffice
                {
                    BranchUID = SalesOffice.BranchUID
                };
                StateHasChanged();
            }
        }

        private async Task SaveFormsData()
        {
            if (await maintainBranchMappingViewModel.SaveStoreDetailsDetails(SalesOffice))
            {
                ShowSuccessSnackBar("Success", "Data Added Successfully");
                await maintainBranchMappingViewModel.GetBranchMappingSalesOffices(SalesOffice.BranchUID);
                StateHasChanged();
                // AddEditSalesOffice = false;
            }
            else
            {
                ShowErrorSnackBar("Network Error", "Unable to Save Details");
                AddEditSalesOffice = false;
            }
        }

        private bool ValidateSalesOfficeData()
        {
            if (string.IsNullOrWhiteSpace(SalesOffice.Code))
            {
                ShowErrorSnackBar("Error", "Code cannot be Empty.");
                return false;
            }
            else if (!ValidateCode(SalesOffice.Code))
            {
                ShowErrorSnackBar("Error", "Code should be unique.");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(SalesOffice.Name))
            {
                ShowErrorSnackBar("Error", "Name cannot be Empty.");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(SalesOffice.WareHouseUID))
            {
                ShowErrorSnackBar("Error", "Warehouse cannot be Empty.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ValidateCode(string code)
        {
            int occurrenceCount = maintainBranchMappingViewModel.CompleteSalesOfficeDetailsList
                 .Count(office => string.Equals(office.Code, code, StringComparison.OrdinalIgnoreCase));
            return (occurrenceCount == 0);
        }

    }
}
