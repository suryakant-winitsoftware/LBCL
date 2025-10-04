using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.StyledXmlParser.Jsoup.Helper;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using Winit.Modules.ApprovalEngine.Model.Interfaces;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.UIState.Classes;
using Winit.Modules.ListHeader.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Modules.User.BL.Interfaces;
using Winit.Modules.User.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
namespace WinIt.Pages.Customer_Details
{
    public partial class CustomerDetails
    {
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public List<FilterModel> ColumnsForFilter;
        public List<DataGridColumn> DataGridColumns { get; set; }
        public bool IsLoaded { get; set; }
        //public List<ISelectionItem> TabItems { get; set; } = new List<ISelectionItem>();
        public List<IOnBoardGridview> onBoardGridviewList { get; set; }
        public List<IOnBoardGridview> onBoardGridviewListCopy { get; set; }
        public List<IAllApprovalRequest> allApprovalRequest { get; set; } = new List<IAllApprovalRequest>();
        public string TabName { get; set; } = "";
        ISelectionItem SelectedTab { get; set; }

        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "OnBoard  New Partner",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="OnBoard  New Partner"},
            }
        };
        private readonly List<ISelectionItem> TabSelectionItems =
       [
           new SelectionItemTab{ Label="Draft", Code = "Draft", UID="1"},
            new SelectionItemTab{ Label="Pending from Asm", Code = "Pending from Asm", UID="2"},
            new SelectionItemTab{ Label="Assigned", Code="Assigned", UID="3"},
            new SelectionItemTab{ Label="Confirmed", Code="Confirmed", UID="4"},
            new SelectionItemTab{ Label="Rejected", Code="Rejected", UID="5"},
        ];

        private SelectionManager TabSelectionManager => new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        public void FilterInitialized()
        {
            ColumnsForFilter = new List<FilterModel>
            {
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Channel Partner Code", ColumnName = "CustomerCode", IsForSearch=true, PlaceHolder="Search By Channel Partner Code", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Channel Partner Name", ColumnName = "CustomerName", IsForSearch=true, PlaceHolder="Search By Channel Partner Name", Width=1000},
                new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Broad Classification", ColumnName = "BroadClassification", IsForSearch=true, PlaceHolder="Search By BroadClassification", Width=1000},
            };
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine(_iAppUser.SelectedJobPosition.UID);
            //await SetHeaderName();
            _loadingService.ShowLoading();
            FilterInitialized();
            _customerDetailsViewModel.PageSize = 20;
            _customerDetailsViewModel.TabName = "Draft";
            await _customerDetailsViewModel.PopulateViewModel();
            onBoardGridviewList = _customerDetailsViewModel.onBoardGridviewList;
            //SelectedTab = new SelectionItem { Label = "Draft", IsSelected = true };
            //TabItems.Add(new SelectionItem { Label = "Draft", IsSelected = true });
            //TabItems.Add(new SelectionItem { Label = "Pending from Asm" });
            //TabItems.Add(new SelectionItem { Label = "Assigned" });
            //TabItems.Add(new SelectionItem { Label = "Confirmed" });
            //TabItems.Add(new SelectionItem { Label = "Rejected" });
            //TabName = "Draft";
            await GetStatus("");
            await GetTabItemsCount(null);
            IsLoaded = true;
            await OnTabSelect(TabSelectionItems.FirstOrDefault()!);

            await GenerateGridColumns();
            await StateChageHandler();
            _loadingService.HideLoading();
            StateHasChanged();
        }
        private async Task StateChageHandler()
        {
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("CustomerDetails", ref ColumnsForFilter, out PageState pageState);
            if (stateRestored && pageState != null && pageState.SelectedTabUID != null)
            {
                ///only work with filters
                await OnFilterApply(_pageStateHandler._currentFilters);
                TabSelectionItems.ForEach(p => p.IsSelected = (p.UID == pageState.SelectedTabUID));
            }
            await OnTabSelect(pageState != null && pageState.SelectedTabUID != null ?
            TabSelectionItems.FirstOrDefault(p => p.IsSelected) : TabSelectionItems.FirstOrDefault());
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("CustomerDetails", TabSelectionManager.GetSelectedSelectionItems().FirstOrDefault()?.UID ?? "");
        }
        public void Dispose()  
        {
            onBoardGridviewList = null;
        }
        public async Task GetStatus(string LinkedItemUID)
        {
            try
            {
                allApprovalRequest = await _customerDetailsViewModel.GetApprovalStatusDetails(LinkedItemUID);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        } 
        public string GetStatusWithUID(string LinkedItemUID)
        {
            try
            {
                IAllApprovalRequest allApprovalRequestobj = allApprovalRequest.Where(p => p.LinkedItemUID == LinkedItemUID).OrderBy(p => p.Level).First();
                if (allApprovalRequestobj != null)
                {
                    return $"Pending from {allApprovalRequestobj.ApproverID} ( Level - {allApprovalRequestobj.Level} )";
                }
                else 
                {
                    return "N/A";
                }
            }
            catch (Exception ex)
            {
                return "N/A";
            }
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = "Channel Partner Code", GetValue = s => ((IOnBoardGridview)s)?.CustomerCode ?? "N/A", SortField="CustomerCode", IsSortable=true},
                new DataGridColumn { Header = "Channel Partner Name ", GetValue = s => ((IOnBoardGridview)s)?.CustomerName ?? "N/A", SortField="CustomerName", IsSortable=true},
                new DataGridColumn {Header = "Broad Classification", GetValue = s =>((IOnBoardGridview) s) ?.BroadClassification ?? "N/A", SortField="BroadClassification", IsSortable=true},
                new DataGridColumn {Header = "Owner Name", GetValue = s =>((IOnBoardGridview) s) ?.OwnerName ?? "N/A", SortField="OwnerName", IsSortable=true},



             };
            if (TabName == "Assigned")
            {
                DataGridColumns.Insert(DataGridColumns.Count - 1, new DataGridColumn { Header = "Status", GetValue = s => GetStatusWithUID(((IOnBoardGridview)s)?.UID), SortField = "Status", IsSortable = true });
            }
            if (TabName == "Draft" || TabName == "Confirmed" || TabName == "Assigned" || TabName == "Pending from Asm")
            {
                DataGridColumns.Insert(DataGridColumns.Count,
                new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    //ButtonActions = this.buttonActions
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Image,
                             URL = "https://qa-fonterra.winitsoftware.com/assets/Images/edit.png",
                            Action = async item => await OnEditUsersClick((IOnBoardGridview)item),

                        },
                        //new ButtonAction
                        //{
                        //     ButtonType = ButtonTypes.Image,
                        //     URL = "https://qa-fonterra.winitsoftware.com/assets/Images/delete.png",
                        //    Action = async item => await OnDeleteUsersClick((IOnBoardGridview)item),

                        //},
                        new ButtonAction
                        {
                             ButtonType = ButtonTypes.Text,
                             Text = "Admin",
                            Action = async item => await OnAdminClick((IOnBoardGridview)item),
                            ConditionalVisibility=item => ((IOnBoardGridview)item)?.IsApproved == true,
                            IsVisible = false,
                        },
                }
                });
            }
        }
        public async Task OnEditUsersClick(IOnBoardGridview customerUID)

        {
            _navigationManager.NavigateTo($"AddCustomers?UID={customerUID.UID}&IsEditOnBoardDetails=true&Tab={TabName}");
        }
        public List<FilterCriteria> FilterCriterias = new List<FilterCriteria>();
        private async Task OnFilterApply(Dictionary<string, string> filterCriteria)
        {
            try
            {
                _pageStateHandler._currentFilters = filterCriteria;
                await _customerDetailsViewModel.OnFilterApply(filterCriteria);
                onBoardGridviewList = _customerDetailsViewModel.onBoardGridviewList;
                //await OnTabSelect(new SelectionItem { Label = TabName });
                await GetTabItemsCount(_customerDetailsViewModel.FilterCriterias);
                StateHasChanged();
            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            try
            {
                ShowLoader();
                TabSelectionItems.ForEach(item => item.IsSelected = false);
                selectionItem.IsSelected = !selectionItem.IsSelected;
                TabName = selectionItem.Label;
                SelectedTab = selectionItem;
                _customerDetailsViewModel.TabName = TabName;
                await _customerDetailsViewModel.PopulateViewModel();
                onBoardGridviewList = _customerDetailsViewModel.onBoardGridviewList;
                await GenerateGridColumns();
                _customerDetailsViewModel.PageNumber = 1;
                StateHasChanged();
                HideLoader();

            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnDeleteUsersClick(IOnBoardGridview customerUID)
        {
            int result = await _customerDetailsViewModel.DeleteOnBoardingDetails(customerUID.UID);
            if (result > 0)
            {
                var DeletedCustomer = _customerDetailsViewModel.onBoardGridviewList.Where(o => o.UID == customerUID.UID).First();
                _customerDetailsViewModel.onBoardGridviewList.Remove(DeletedCustomer);
                _tost.Add("Delete", "Customer Deleted Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
            }
            else
            {
                ShowErrorSnackBar("Error", "Failed to Delete...");
            }
            StateHasChanged();
        }
        public async Task OnAdminClick(IOnBoardGridview customerUID)
        {
            try
            {
                _navigationManager.NavigateTo($"distributoradmin?OrgUID={customerUID.UID}&Status={"Active"}");
            }
            catch (Exception ex)
            {

            }
        }
        public async Task AddCustomer()
        {
            _navigationManager.NavigateTo($"AddCustomers");

        }
        private async Task GetTabItemsCount(List<FilterCriteria> filterCriterias)
        {
            if (filterCriterias != null && filterCriterias.Exists(e => e.Name == "Status"))
            {
                filterCriterias.RemoveAll(e => e.Name == "Status");
            }
            IDictionary<string, int> statusDict = await _customerDetailsViewModel.GetTabItemsCount(filterCriterias);

            int allCount = 0;
            // foreach (KeyValuePair<string, int> item in statusDict)
            // {
            //     ISelectionItem? tab = TabSelectionItems.Find(e => e.Code == item.Key);
            //     if (tab is SelectionItemTab selectionItemTab)
            //     {
            //         selectionItemTab.Count = item.Value;
            //     }
            //     allCount += item.Value;
            // }

            foreach (SelectionItemTab tab in TabSelectionItems)
            {
                if (statusDict.TryGetValue(tab.Code, out int count))
                {
                    tab.Count = count;
                }
                else
                {
                    tab.Count = 0;
                }
            }
            //ISelectionItem? allTab = TabSelectionItems.Find(e => e.UID == "1");
            //if (allTab is SelectionItemTab allSelectionItemTab)
            //{
            //    allSelectionItemTab.Count = allCount;
            //}
        }
        private async Task OnSortClick(SortCriteria sortCriteria)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();

                await _customerDetailsViewModel.OnSorting(sortCriteria);
                HideLoader();
            });
        }

        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();
                await _customerDetailsViewModel.PageIndexChanged(pageNumber);
                onBoardGridviewList = _customerDetailsViewModel.onBoardGridviewList;
                HideLoader();
            });
        }

    }
}
