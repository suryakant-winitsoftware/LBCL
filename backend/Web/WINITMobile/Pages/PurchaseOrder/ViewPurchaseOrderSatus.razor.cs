using Winit.Modules.Common.BL;
using Winit.Modules.PurchaseOrder.Model.Constatnts;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.PurchaseOrder
{
    public partial class ViewPurchaseOrderSatus : BaseComponentBase
    {
        private readonly List<FilterModel> FilterColumns = [];

        private readonly List<ISelectionItem> TabSelectionItems =
        [
            new SelectionItemTab
        {
            Label = "ALL",
            Code = "",
            UID = "1"
        },
        new SelectionItemTab
        {
            Label = "Saved/Draft",
            Code = PurchaseOrderStatusConst.Draft,
            UID = "2"
        },
        new SelectionItemTab
        {
            Label = "Approval Pending-DMS",
            Code = PurchaseOrderStatusConst.PendingForApproval,
            UID = "3"
        },
        new SelectionItemTab
        {
            Label = "Oracle Order Status",
            Code = PurchaseOrderStatusConst.InProcessERP,
            UID = "4"
        },
        new SelectionItemTab
        {
            Label = "Cancelled",
            Code = PurchaseOrderStatusConst.CancelledByCMI,
            UID = "5"
        },
    ];

        private SelectionManager TabSelectionManager =>
            new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);

        private List<DataGridColumn> GridColumns = [];


        protected override async Task OnInitializedAsync()
        {
            GridColumns =
            [
                new DataGridColumn
            {
                Header = "Channel Partner",
                IsSortable = true,
                GetValue = s =>
                    $"[{((IPurchaseOrderHeaderItem)s).ChannelPartnerCode}]{((IPurchaseOrderHeaderItem)s).ChannelPartnerName}",
                SortField = "channelpartnercode"
            },
            new DataGridColumn
            {
                Header = "DMS Purchase Order No",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OrderNumber,
                SortField = "ordernumber",
                Style = "he"
            },
            new DataGridColumn
            {
                Header = "DMS Creation Date",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OrderDate.ToString("dd MMM yyyy HH:mm:ss"),
                SortField = "orderdate"
            },
            new DataGridColumn
            {
                Header = "DMS Approval Date",
                IsSortable = true,
                GetValue = s =>
                    CommonFunctions.IsDateNull(((IPurchaseOrderHeaderItem)s).CPEConfirmDateTime)
                        ? "N/A"
                        : ((IPurchaseOrderHeaderItem)s).CPEConfirmDateTime!.ToString("dd MMM yyyy HH:mm:ss"),
                SortField = "cpeconfirmdatetime"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Number",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OracleNo ?? "N/A",
                SortField = "OracleNo"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Date",
                IsSortable = true,
                GetValue = s =>
                    CommonFunctions.IsDateNull(((IPurchaseOrderHeaderItem)s).OracleOrderdate)
                        ? "N/A"
                        : ((IPurchaseOrderHeaderItem)s).OracleOrderdate.Value.ToString("dd MMM yyyy"),
                SortField = "oracleorderdate"
            },
            new DataGridColumn
            {
                Header = "Net Amount",
                IsSortable = true,
                GetValue = s => CommonFunctions.FormatNumberInIndianStyle(((IPurchaseOrderHeaderItem)s).NetAmount),
                SortField = "netamount",
                Class = "cls_purchase_amount"
            },
            new DataGridColumn
            {
                Header = "ASM Name",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).ReportingEmpName,
                SortField = "ReportingEmpName"
            },
            new DataGridColumn
            {
                Header = "Oracle Order Status",
                IsSortable = true,
                GetValue = s => ((IPurchaseOrderHeaderItem)s).OracleOrderStatus ?? "N/A",
                SortField = "OracleOrderStatus"
            },
            new DataGridColumn
            {
                IsButtonColumn = true,
                Header = "Action",
                IsSortable = false,
                ButtonActions =
                [
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/edit.png",
                        Action = e => OnEditbtnClick((IPurchaseOrderHeaderItem)e)
                    }
                ],
            }
            ];

            _viewModel.PageNumber = 1;
            _viewModel.PageSize = 50;

            await OnTabSelect(TabSelectionItems.FirstOrDefault()!);
            await GetTabItemsCount();
            _ = _viewModel.LoadChannelPartner();
            PrepareFilterData();
        }

        private void PrepareFilterData()
        {
            FilterColumns.AddRange(
            new List<FilterModel>
            {
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Multiple,
                Label = "Channel Partner",
                ColumnName = "OrgUID",
                DropDownValues = _viewModel.ChannelPartnerSelectionItems
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                Label = "DMS Purchase Order No",
                ColumnName = "OrderNumber"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,
                Label = "Oracle Order Number",
                ColumnName = "OracleNo"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "DMS Creation Start Date",
                ColumnName = "startdate"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.Date,
                Label = "DMS Creation End Date",
                ColumnName = "enddate"
            },
            new()
            {
                FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,
                DropDownValues = _viewModel.OracleOrderStatusSelectionItems,
                SelectionMode = Winit.Shared.Models.Enums.SelectionMode.Multiple,
                Label = "Oracle Order Status",
                ColumnName = "OracleOrderStatus"
            },
            });
        }

        private void CreateNewPurchaseOrder()
        {
            _navigationManager.NavigateTo("purchaseorder");
        }

        private async Task GetTabItemsCount()
        {
            IDictionary<string, int> statusDict = await _viewModel
                .GetTabItemsCount(_viewModel.FilterCriterias);
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
                    allCount += count;
                }
                else
                {
                    tab.Count = 0;
                }
            }
            ISelectionItem? allTab = TabSelectionItems.Find(e => e.UID == "1");
            if (allTab is SelectionItemTab allSelectionItemTab)
            {
                allSelectionItemTab.Count = allCount;
            }
        }

        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            ShowLoader();
            if (selectionItem != null && !selectionItem.IsSelected)
            {
                TabSelectionManager.Select(selectionItem);
                _viewModel.PageNumber = 1;
                await FetchDataAsync(selectionItem.Code!);
                DataGridColumn? column = GridColumns.Find(e => e.Header == "ASM Name" || e.Header == "Created By" || e.Header == "ASM Name / Created By");
                if (selectionItem.UID == "1" && column != null)
                {
                    column.Header = "ASM Name / Created By";
                    column.GetValue = (item) =>
                    {
                        var item1 = (IPurchaseOrderHeaderItem)item;
                        return item1.Status == PurchaseOrderStatusConst.Draft ? $"[{item1.CreatedByEmpCode}] {item1.CreatedByEmpName}" : $"[{item1.ReportingEmpCode}] {item1.ReportingEmpName}";
                    };
                    column.SortField = "ReportingEmpName";
                }
                else if (selectionItem.UID == "2" && column != null)
                {
                    column.Header = "Created By";
                    column.GetValue = (item) =>
                        $"[{(item as IPurchaseOrderHeaderItem).CreatedByEmpCode}] {(item as IPurchaseOrderHeaderItem).CreatedByEmpName}";
                    column.SortField = "CreatedByEmpName";
                }
                else
                {
                    column.Header = "ASM Name";
                    column.GetValue = (item) =>
                        $"[{(item as IPurchaseOrderHeaderItem).ReportingEmpCode}] {(item as IPurchaseOrderHeaderItem).ReportingEmpName}";
                    column.SortField = "ReportingEmpName";
                }
            }
            StateHasChanged();
            HideLoader();
        }

        private async Task FetchDataAsync(string status)
        {
            try
            {
                _viewModel.Status = status;
                await _viewModel.PopulateViewModel();
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Failed to fetch purchase order headers", ex.Message);
            }
        }

        private void OnEditbtnClick(IPurchaseOrderHeaderItem purchaseOrderHeaderItem)
        {
            _navigationManager.NavigateTo($"purchaseorder/{purchaseOrderHeaderItem.UID}");
        }

        private async Task OnSortClick(SortCriteria sortCriteria)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();

                await _viewModel.OnSorting(sortCriteria);
                HideLoader();
            });
        }

        private async Task PageIndexChanged(int pageNumber)
        {
            await InvokeAsync(async () =>
            {
                ShowLoader();

                await _viewModel.PageIndexChanged(pageNumber);
                HideLoader();
            });
        }

        private static string GetStatus(string status)
        {
            return status switch
            {
                PurchaseOrderStatusConst.Draft => "Draft",
                PurchaseOrderStatusConst.PendingForApproval => "Pending For Approval",
                PurchaseOrderStatusConst.InProcessERP => "In Process ERP",
                PurchaseOrderStatusConst.Invoiced => "Inoviced",
                PurchaseOrderStatusConst.Shipped => "Shipped",
                PurchaseOrderStatusConst.CancelledByCMI => "Cancelled By CMI",
                _ => "Draft",
            };
        }

        private static string GetOracleOrderStatus(string status)
        {
            return status switch
            {
                _ => "N/A",
            };
        }
        private async Task OnFilterClick(Dictionary<string, string> filtersDict)
        {
            try
            {
                ShowLoader();
                var filters = filtersDict.Where(_ => !string.IsNullOrEmpty(_.Value)).Select(e =>
                {
                    if (e.Key == "startdate") return new FilterCriteria("OrderDate", e.Value, FilterType.GreaterThanOrEqual, typeof(DateTime));
                    else if (e.Key == "enddate") return new FilterCriteria("OrderDate", e.Value, FilterType.LessThanOrEqual, typeof(DateTime));
                    else if (e.Key == "OracleNo" || e.Key == "orderno") return new(e.Key, e.Value, FilterType.Like);
                    else if (e.Key == "OrgUID") return new(e.Key, e.Value.Split(","), FilterType.In);
                    else if (e.Key == "OracleOrderStatus") return new FilterCriteria(e.Key, e.Value.Split(","), FilterType.In);
                    return new FilterCriteria(e.Key, e.Value, FilterType.Equal);
                }).ToList();
                await _viewModel.ApplyFilter(filters);
                await GetTabItemsCount();
            }
            catch (Exception e)
            {
            }
            finally
            {
                HideLoader();
            }
        }
    }
}
