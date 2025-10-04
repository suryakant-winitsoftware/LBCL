using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Tally.TallyMaster
{
    public partial class SalesInvoiceLineMaster : BaseComponentBase
    {
        public bool IsInitialised { get; set; } = false;
        public string SalesInvoiceUID { get; set;} = string.Empty;
        public bool IsItemSelectedToShow { get; set; } = false;
        public List<DataGridColumn> DataGridColumns { get; set; } = new List<DataGridColumn>();
        public List<DataGridColumn> SalesInvoiceResultDataGridColumns { get; set; } = new List<DataGridColumn>();


        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Sales Invoice Line Master ",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo=1,Text="Sales Invoice Master", URL = "SalesInvoiceMaster", IsClickable = true },
                new BreadCrumModel(){SlNo=1,Text="Sales Invoice Line Master"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            SalesInvoiceUID = _commonFunctions.GetParameterValueFromURL("SalesInvoiceUID");
            if (!string.IsNullOrEmpty(SalesInvoiceUID) )
            {
               await tallyMasterViewModel.GetSalesInvoiceLineMasterGridDataByUID(SalesInvoiceUID);
            }
            GenerateGridColumns();
           // await GenerateSalesInvoiceResultDataGridColumns();
            IsInitialised = true;
            HideLoader();
        }

        private void GenerateSalesInvoiceResultDataGridColumns()
        {
            throw new NotImplementedException();
        }

        private void GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {
               // new DataGridColumn { Header = "DmsUid", GetValue = item => ((ITallySalesInvoiceLineMaster)item).DmsUid ?? "N/A" },
               // new DataGridColumn { Header = "Guid", GetValue = item => ((ITallySalesInvoiceLineMaster)item).Guid ?? "N/A" },
                new DataGridColumn { Header = "Voucher Number", GetValue = item => ((ITallySalesInvoiceLineMaster)item).VoucherNumber ?? "N/A" },
                new DataGridColumn { Header = "Stock Item Name", GetValue = item => ((ITallySalesInvoiceLineMaster)item).StockItemName ?? "N/A" },
                //new DataGridColumn { Header = "Rate", GetValue = item => ((ITallySalesInvoiceLineMaster)item).Rate ?? "N/A" },
                new DataGridColumn { Header = "Amount", GetValue = item => ((ITallySalesInvoiceLineMaster)item).Amount ?? "N/A" },
                new DataGridColumn { Header = "Actual Quantity", GetValue = item => ((ITallySalesInvoiceLineMaster)item).ActualQty ?? "N/A" },
                new DataGridColumn { Header = "Billed Quantity", GetValue = item => ((ITallySalesInvoiceLineMaster)item).BilledQty ?? "N/A" },
               // new DataGridColumn { Header = "GST", GetValue = item => ((ITallySalesInvoiceLineMaster)item).Gst.ToString("F2") ?? "N/A" },
               // new DataGridColumn { Header = "Discount Percentage", GetValue = item => ((ITallySalesInvoiceLineMaster)item).DiscountPercentage ?? "N/A" },
                new DataGridColumn { Header = "Quantity", GetValue = item => ((ITallySalesInvoiceLineMaster)item).Qty.ToString("F2") ?? "N/A" },
                new DataGridColumn { Header = "Unit Price", GetValue = item => ((ITallySalesInvoiceLineMaster)item).UnitPrice.ToString("F2") ?? "N/A" },
                new DataGridColumn { Header = "Total Amount", GetValue = item => ((ITallySalesInvoiceLineMaster)item).TotalAmount.ToString("F2") ?? "N/A" },
               // new DataGridColumn { Header = "Total Discount", GetValue = item => ((ITallySalesInvoiceLineMaster)item).TotalDiscount.ToString("F2") ?? "N/A" },
               // new DataGridColumn { Header = "Total Tax", GetValue = item => ((ITallySalesInvoiceLineMaster)item).TotalTax.ToString("F2") ?? "N/A" },
                new DataGridColumn { Header = "Net Amount", GetValue = item => ((ITallySalesInvoiceLineMaster)item).NetAmount.ToString("F2") ?? "N/A" },
                new DataGridColumn { Header = "Action", IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/view.png",
                            Action = item => OnViewClick((ITallySalesInvoiceLineMaster)item)
                        },
                    }},
            };
        }
        private async Task OnViewClick(ITallySalesInvoiceLineMaster item)
        {
            ShowLoader();
          //  await tallyMasterViewModel.GetTallySalesInvoiceLineMasterItemDetails(item.);
            IsItemSelectedToShow = true;
            HideLoader();
        }
        private void OnBackButtonClick()
        {
            IsItemSelectedToShow = false;
        }
    }
}
