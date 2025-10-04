using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Interfaces;

public interface IInvoiceBL
{
    Task<PagedResponse<IInvoiceHeaderView>> GetAllInvoices(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired, string jobPositionUID);
    Task<IInvoiceMaster> GetInvoiceMasterByInvoiceUID(string invoiceUID);
    Task<PagedResponse<IProvisioningCreditNoteView>> GetInvoiceApproveSatsusDetails(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired, bool Status);

    Task<int> UpdateApprovedStatus(List<IProvisioningCreditNoteView> provisioningItems);
    Task<PagedResponse<IOutstandingInvoiceReport>> GetOutstandingInvoiceReportData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, bool status);
    Task<List<IInvoiceView>> GetInvoicesForReturnOrder(InvoiceListRequest invoiceListRequest);
    Task<PagedResponse<IProvisionComparisonReportView>> GetProvisionComparisonReport(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    Task<List<IProvisionDataDMS>> CreateProvision(string invoiceUID);
}
