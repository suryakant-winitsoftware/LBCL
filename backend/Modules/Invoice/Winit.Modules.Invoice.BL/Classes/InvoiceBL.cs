using Winit.Modules.Invoice.BL.Interfaces;
using Winit.Modules.Invoice.DL.Interfaces;
using Winit.Modules.Invoice.Model.Classes;
using Winit.Modules.Invoice.Model.Interfaces;
using Winit.Modules.ProvisionComparisonReport.Model.Interfaces;
using Winit.Modules.Provisioning.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Invoice.BL.Classes;

public class InvoiceBL : IInvoiceBL
{
    private readonly IInvoiceDL _invoiceDL;

    public InvoiceBL(IInvoiceDL invoiceDL)
    {
        _invoiceDL = invoiceDL;
    }


    public async Task<PagedResponse<IInvoiceHeaderView>> GetAllInvoices(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired,string jobPositionUID)
    {
        return await _invoiceDL.GetAllInvoices(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,jobPositionUID);
    }

    public async Task<IInvoiceMaster> GetInvoiceMasterByInvoiceUID(string invoiceUID)
    {
        return await _invoiceDL.GetInvoiceMasterByInvoiceUID(invoiceUID);
    }
    public async Task<PagedResponse<IInvoiceApprove>> GetInvoiceApproveSatsusDetails(List<SortCriteria>? sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired)
    {
        return await _invoiceDL.GetInvoiceApproveSatsusDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<PagedResponse<IProvisioningCreditNoteView>> GetInvoiceApproveSatsusDetails(List<SortCriteria>? sortCriterias, int pageNumber,
       int pageSize, List<FilterCriteria>? filterCriterias, bool isCountRequired, bool Status)
    {
        return await _invoiceDL.GetInvoiceApproveSatsusDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired, Status);
    }

    public async Task<int> UpdateApprovedStatus(List<IProvisioningCreditNoteView> provisioningItems)
    {
        return await _invoiceDL.UpdateApprovedStatus(provisioningItems);
    }

    public async Task<PagedResponse<IOutstandingInvoiceReport>> GetOutstandingInvoiceReportData(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, bool status)
    {
        return await _invoiceDL.GetOutstandingInvoiceReportData(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }

    public async Task<List<IInvoiceView>> GetInvoicesForReturnOrder(InvoiceListRequest invoiceListRequest)
    {
        return await _invoiceDL.GetInvoicesForReturnOrder(invoiceListRequest);
    }
    public async Task<PagedResponse<IProvisionComparisonReportView>> GetProvisionComparisonReport(
        List<SortCriteria> sortCriterias,
        int pageNumber,
        int pageSize,
        List<FilterCriteria> filterCriterias,
        bool isCountRequired)
    {
        return await _invoiceDL.GetProvisionComparisonReport(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<List<IProvisionDataDMS>> CreateProvision(string invoiceUID)
    {
        return await _invoiceDL.CreateProvision(invoiceUID);
    }
}
