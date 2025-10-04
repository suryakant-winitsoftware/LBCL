using System;
using System.Collections.Generic;
using Winit.Modules.ETL.Invoice.DL.Interfaces;

namespace Winit.Modules.ETL.Invoice.DL.Classes
{
    public class InvoiceQueryProviderFactory : IInvoiceQueryProviderFactory
    {
        private readonly Dictionary<string, IInvoiceQueryProvider> _sourceProviders;
        private readonly Dictionary<string, IInvoiceQueryProvider> _destinationProviders;

        public InvoiceQueryProviderFactory()
        {
            _sourceProviders = new Dictionary<string, IInvoiceQueryProvider>(StringComparer.OrdinalIgnoreCase);
            _destinationProviders = new Dictionary<string, IInvoiceQueryProvider>(StringComparer.OrdinalIgnoreCase);
        }

        public void RegisterSourceProvider(string dbType, IInvoiceQueryProvider provider)
        {
            if (string.IsNullOrEmpty(dbType))
                throw new ArgumentNullException(nameof(dbType));
            
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _sourceProviders[dbType] = provider;
        }

        public void RegisterDestinationProvider(string dbType, IInvoiceQueryProvider provider)
        {
            if (string.IsNullOrEmpty(dbType))
                throw new ArgumentNullException(nameof(dbType));
            
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _destinationProviders[dbType] = provider;
        }

        public IInvoiceQueryProvider GetSourceProvider(string dbType)
        {
            if (string.IsNullOrEmpty(dbType))
                throw new ArgumentNullException(nameof(dbType));

            if (!_sourceProviders.TryGetValue(dbType, out var provider))
                throw new ArgumentException($"No source provider registered for database type: {dbType}");

            return provider;
        }

        public IInvoiceQueryProvider GetDestinationProvider(string dbType)
        {
            if (string.IsNullOrEmpty(dbType))
                throw new ArgumentNullException(nameof(dbType));

            if (!_destinationProviders.TryGetValue(dbType, out var provider))
                throw new ArgumentException($"No destination provider registered for database type: {dbType}");

            return provider;
        }
    }
} 