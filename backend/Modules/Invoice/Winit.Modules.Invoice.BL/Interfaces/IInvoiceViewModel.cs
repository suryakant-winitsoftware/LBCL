using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Interfaces;

public interface IInvoiceViewModel
{
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<IInvoiceHeaderView> InvoiceHeaderViews { get; set; }
    public IInvoiceMaster InvoiceMaster { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<ISelectionItem> ChannelPartnerSelectionItem { get; set; }
    Task PopulateViewModel();
    Task PageIndexChanged(int index);
    Task OnSortClick(SortCriteria sortCriteria);
    Task ApplyFilter(List<FilterCriteria> filterCriterias);
    Task LoadInvoiceMasterByInoviceNo(string inoviceNo);
    Task LoadChannelPartner();

    //Ravichandra Added for OutstandingInvoiceReport
    public List <IOutstandingInvoiceReport> OutstandingInvoiceReportDataList {  get; set; }
    Task GetOutstandingInvoiceReportData();
}
