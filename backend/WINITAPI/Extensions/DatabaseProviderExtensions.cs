using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace WINITAPI.Extensions
{
    public static class DatabaseProviderExtensions
    {
        private static bool _hasLogged = false;

        public static IServiceCollection AddDatabaseProvider<TInterface, TPgSqlImpl, TMsSqlImpl>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TInterface : class
            where TPgSqlImpl : class, TInterface
            where TMsSqlImpl : class, TInterface
        {
            string dbProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

            // Log once for debugging
            if (!_hasLogged)
            {
                Console.WriteLine($"[DatabaseProvider] Configuration value: {dbProvider}");
                _hasLogged = true;
            }

            if (dbProvider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[DatabaseProvider] Registering MSSQL: {typeof(TInterface).Name} -> {typeof(TMsSqlImpl).Name}");
                services.AddTransient<TInterface, TMsSqlImpl>();
            }
            else
            {
                Console.WriteLine($"[DatabaseProvider] Registering PostgreSQL: {typeof(TInterface).Name} -> {typeof(TPgSqlImpl).Name}");
                services.AddTransient<TInterface, TPgSqlImpl>();
            }

            return services;
        }

        public static string GetDatabaseProvider(this IConfiguration configuration)
        {
            return configuration["DatabaseProvider"] ?? "PostgreSQL";
        }
    }
}
