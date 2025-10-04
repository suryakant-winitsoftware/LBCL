
using Microsoft.AspNetCore.Components;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.ManageInvoice;

partial class ViewInvoice
{
    [Parameter]
    public string InvoiceUID { get; set; } = string.Empty;

    private IInvoiceLineView SelectedItem = new InvoiceLineView();

    private List<DataGridColumn> GridColumns = [];
    private Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel
    {
        BreadcrumList =
        [
            new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel() { SlNo = 1, Text = "View Invoice", IsClickable = false },
        ],
        HeaderText = "Manage Invoices"
    };

    private bool IsShowSerialPopUP = false;
    protected async override Task OnInitializedAsync()
    {
        ShowLoader();
        GridColumns =
        [
            new DataGridColumn { Header = "Item Code & Name", IsSortable = true, GetValue = s => $"[{((IInvoiceLineView)s).ItemCode}]{((IInvoiceLineView)s).ItemName} ", SortField="itemname" },
            new DataGridColumn { Header = "Unit Price", IsSortable = true,GetValue = s => CommonFunctions.RoundForSystemWithoutZero(((IInvoiceLineView)s).UnitPrice,_appSetting.RoundOffDecimal)  , SortField="unitprice"    },
            new DataGridColumn { Header = "Ordered <br> Qty", Style="", IsSortable = true,GetValue = s => CommonFunctions.RoundForSystem(((IInvoiceLineView)s).OrderedQty,0)  , SortField="orderedqty"    },
            new DataGridColumn { Header = "Shipped <br> Qty", Style="",IsSortable = true,GetValue = s => CommonFunctions.RoundForSystem(((IInvoiceLineView)s).ShippedQty,0)  , SortField="shippedqty"    },
            new DataGridColumn { Header = "Cancelled <br> Qty", Style="",IsSortable = true,GetValue = s => CommonFunctions.RoundForSystem(((IInvoiceLineView)s).CancelledQty,0)  , SortField="cancelledqty"    },
            new DataGridColumn { Header = "Total Amount", IsSortable = true,GetValue = s => CommonFunctions.RoundForSystemWithoutZero(((IInvoiceLineView)s).TotalAmount,_appSetting.RoundOffDecimal)  , SortField="totalamount"    },
            // new DataGridColumn { Header = "Total Tax", IsSortable = true,GetValue = s => CommonFunctions.RoundForSystemWithoutZero(((IInvoiceLineView)s).TotalTax,_appSetting.RoundOffDecimal)  , SortField="totaltax"    },
            new DataGridColumn
            {
                IsButtonColumn = true,
                Header = "Action", 
                IsSortable = false,
                ButtonActions =
                [   new ButtonAction
                    {
                        ButtonType = ButtonTypes.Text,
                        Text = "View",
                        Action = e => OnViewClick((IInvoiceLineView) e)
                    }
                ],
            }
        ];
        _viewModel.PageNumber = 1;
        _viewModel.PageSize = 50;
        await _viewModel.LoadInvoiceMasterByInoviceNo(InvoiceUID);
        HideLoader();
    }

    private void OnViewClick(IInvoiceLineView invoiceLineView)
    {
        SelectedItem = invoiceLineView;
        IsShowSerialPopUP = true;
        StateHasChanged();
    }
    private void OnDownloadClick()
    {
        if (string.IsNullOrEmpty(_viewModel.InvoiceMaster?.Invoiceheader?.InvoiceURL))
        {
            ShowErrorSnackBar("No Invoice Url Found..");
            return;
        }
        _navigationManager.NavigateTo(Path.Combine(_appConfigs.ApiDataBaseUrl, _viewModel.InvoiceMaster?.Invoiceheader?.InvoiceURL!));
    }
}
