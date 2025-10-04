using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Winit.Modules.ETL.Invoice.BL.Classes;
using Winit.Modules.ETL.Invoice.BL.Interfaces;
using Winit.Modules.ETL.Invoice.DL.Classes;
using Winit.Modules.ETL.Invoice.DL.Interfaces;

namespace Winit.Modules.ETL.Invoice.BL.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInvoiceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database settings
            services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

            // Register connection factory
            services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

            // Register query providers
            services.AddScoped<MSSQLInvoiceQueryProvider>();
            services.AddScoped<PGSQLInvoiceQueryProvider>();

            // Register and configure query provider factory
            services.AddSingleton<IInvoiceQueryProviderFactory>(sp =>
            {
                var factory = new InvoiceQueryProviderFactory();
                var dbSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
                
                // Register source provider based on configuration
                var sourceProvider = dbSettings.SourceDbType.ToUpper() switch
                {
                    "MSSQL" => sp.GetRequiredService<MSSQLInvoiceQueryProvider>(),
                    "PGSQL" => sp.GetRequiredService<PGSQLInvoiceQueryProvider>(),
                    _ => throw new ArgumentException($"Unsupported source database type: {dbSettings.SourceDbType}")
                };
                factory.RegisterSourceProvider(dbSettings.SourceDbType, sourceProvider);

                // Register destination provider based on configuration
                var destinationProvider = dbSettings.DestinationDbType.ToUpper() switch
                {
                    "MSSQL" => sp.GetRequiredService<MSSQLInvoiceQueryProvider>(),
                    "PGSQL" => sp.GetRequiredService<PGSQLInvoiceQueryProvider>(),
                    _ => throw new ArgumentException($"Unsupported destination database type: {dbSettings.DestinationDbType}")
                };
                factory.RegisterDestinationProvider(dbSettings.DestinationDbType, destinationProvider);
                
                return factory;
            });

            // Register DL implementation
            var dbSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
            
            // Validate database types
            ValidateDatabaseType(dbSettings.SourceDbType, "Source");
            ValidateDatabaseType(dbSettings.DestinationDbType, "Destination");

            // Register CompositeInvoiceDL for all scenarios
            services.AddScoped<IInvoiceDL>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CompositeInvoiceDL>>();
                var queryProviderFactory = sp.GetRequiredService<IInvoiceQueryProviderFactory>();
                return new CompositeInvoiceDL(logger, queryProviderFactory, dbSettings.SourceDbType, dbSettings.DestinationDbType);
            });

            // Register BL services
            services.AddScoped<IInvoiceService, InvoiceService>();

            return services;
        }

        private static void ValidateDatabaseType(string dbType, string context)
        {
            if (string.IsNullOrEmpty(dbType))
            {
                throw new ArgumentException($"{context} database type is not configured");
            }

            var upperDbType = dbType.ToUpper();
            if (upperDbType != "MSSQL" && upperDbType != "PGSQL")
            {
                throw new ArgumentException($"Unsupported {context} database type: {dbType}");
            }
        }
    }
}