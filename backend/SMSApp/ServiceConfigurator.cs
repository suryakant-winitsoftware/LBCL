namespace SMSApp
{
    public static class ServiceConfigurator
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            _ = services.AddTransient<ProcessJobs>();
        }
    }
}
