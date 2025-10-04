using System;
using System.Collections.Generic;
using Winit.Modules.ETL.Invoice.Model.Interfaces;

namespace Winit.Modules.ETL.Invoice.DL.Interfaces
{
    public interface IInvoiceQueryProvider
    {
        string GetInvoiceQuery();
        string GetInvoicesByDateRangeQuery();
        string GetUpdateInvoiceQuery();
        string GetInsertInvoiceQuery();
        string GetUpdateInvoiceLinesQuery();
        string GetInsertInvoiceLinesQuery();
        string GetDeleteInvoiceLinesQuery();
        string GetInvoiceExistsQuery();
        string GetExistingInvoiceLinesQuery();
    }
} 