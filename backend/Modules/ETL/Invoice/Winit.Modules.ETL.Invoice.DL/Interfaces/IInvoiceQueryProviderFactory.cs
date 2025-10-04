using System;
using System.Collections.Generic;

namespace Winit.Modules.ETL.Invoice.DL.Interfaces
{
    public interface IInvoiceQueryProviderFactory
    {
        void RegisterSourceProvider(string dbType, IInvoiceQueryProvider provider);
        void RegisterDestinationProvider(string dbType, IInvoiceQueryProvider provider);
        IInvoiceQueryProvider GetSourceProvider(string dbType);
        IInvoiceQueryProvider GetDestinationProvider(string dbType);
    }
} 