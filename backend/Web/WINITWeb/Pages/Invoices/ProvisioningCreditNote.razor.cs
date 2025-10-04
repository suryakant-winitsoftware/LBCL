using DocumentFormat.OpenXml.Vml;
using Winit.Modules.Common.BL;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.SalesOrder.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace WinIt.Pages.Invoices
{
    public partial class ProvisioningCreditNote
    {
        private List<IProvisioningCreditNoteView> selectedItems = new List<IProvisioningCreditNoteView>();
        private readonly Winit.UIComponents.Web.Filter.Filter? filterRef;
        string ActiveTab = "";
        private bool IsApproveTabSelected { set; get; } = false;
        public List<FilterModel>? ColumnsForFilter;
       // public string SelectedTab = "Pending";
        private SelectionManager SelectedTab { get; set; }
        private readonly List<ISelectionItem> TabSelectionItems =
            [
            new SelectionItemTab{ Label="Pending", ExtData =false, UID="1"},
            new SelectionItemTab{ Label=" Approved", ExtData=true, UID="2"},
        ];

        private SelectionManager TabSelectionManager => new(TabSelectionItems, Winit.Shared.Models.Enums.SelectionMode.Single);
        public void FilterInitialized()
        {


            ColumnsForFilter = new List<FilterModel>
        {
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = "Channel Partner",ColumnName = "OrgUID"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = "Invoice No",ColumnName = "invoice_number"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label ="Start Date",ColumnName = "invoice_date"},
            new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label ="End Date",ColumnName = "invoice_date"}
        };
        }
        private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
        {
            await _viewmodel.OnFilterApply(ColumnsForFilter, keyValuePairs, SelectedTab.GetSelectedSelectionItems().FirstOrDefault().ToString());
        }

        private List<DataGridColumn> GridColumns = new List<DataGridColumn>();
        private readonly IDataService DataService = new DataServiceModel()
        {
            HeaderText = "ProvisioningCreditNote",
            BreadcrumList =
            [
            new BreadCrumModel(){SlNo=2, Text="ProvisioningCreditNote"},
        ]
        };

        protected override async Task OnInitializedAsync()
        {
            FilterInitialized();
            await _viewmodel.LoadDataAsync();
            GridColumns =
           [
            new DataGridColumn { Header = "Channel partner", IsSortable = true, GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).ChannelPartnerName ?? "N/A", SortField="OrgUID" },
            new DataGridColumn { Header = "Document Type", IsSortable = true,GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).LinkedItemType ?? "N/A" , SortField="LinkedItemType"    },
            new DataGridColumn { Header = "Document No", IsSortable = true,GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).LinkedItemUid ?? "N/A" , SortField="LinkedItemUid"    },
            new DataGridColumn { Header = "Invoice Date", IsSortable = true,GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).InvoiceDate.ToString("dd MMM yyyy hh:mm:ss tt") ?? "N/A", SortField="Invoicedate" },
            new DataGridColumn { Header = "Creadit Note Provision1", IsSortable = true, GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).CreditNoteAmount1.ToString() ?? "N/A", SortField="CreditNoteAmount1" },
            new DataGridColumn { Header = "Creadit Note Provision2", IsSortable = true, GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).CreditNoteAmount2.ToString() ?? "N/A", SortField="CreditNoteAmount2" },
            new DataGridColumn { Header = "Creadit Note Provision3", IsSortable = true, GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).CreditNoteAmount3.ToString() ?? "N/A", SortField="CreditNoteAmount3" },
            new DataGridColumn { Header = "Creadit Note Standing", IsSortable = true,GetValue = s => ((Winit.Modules.Invoice.Model.Interfaces.IProvisioningCreditNoteView)s).CreditNoteAmount4.ToString() ?? "N/A", SortField="CreditNoteAmount4" },
            ];
            SetVariables();
        }
        public void SetVariables()
        {
            ActiveTab = Winit.Modules.Base.Model.CommonConstant.SeqType;
            SelectedTab = new SelectionManager(TabSelectionItems, SelectionMode.Single);
            TabSelectionItems[0].IsSelected = ActiveTab == Winit.Modules.Base.Model.CommonConstant.SeqType;
        }
        private async Task OnRowSelected(IProvisioningCreditNoteView selectedRow, bool isSelected)
        {
            if (isSelected)
            {

                selectedItems.Add(selectedRow);
            }
            else
            {
                selectedItems.Remove(selectedRow);
            }
        }

        private async Task ApproveSelected()
        {
            if(await _alertService.ShowConfirmationReturnType("Confirm","Are You Sure Want To Approve"))
            {
                await _viewmodel.ApproveSelectedAsync();
            } 
        }
        public async Task OnTabSelect(ISelectionItem selectionItem)
        {
            if (selectionItem != null)
            {
                bool status = CommonFunctions.GetBooleanValue(selectionItem!.ExtData);
                IsApproveTabSelected = status;
                SelectedTab.Select(selectionItem);
                ActiveTab = selectionItem.Code;
                await _viewmodel.LoadDataAsync(status); 
            }
        }
        private async void AfterCheckBoxSelection(HashSet<object> hashSet)
        {
            foreach (var item in _viewmodel.DisplayCreditNoteList.OfType<IProvisioningCreditNoteView>())
            {
                if (hashSet.Contains(item))
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
            await Task.CompletedTask;
        }


    }
}
