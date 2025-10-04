using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Serilog;
using WINITSyncManager.Common;
using WINITSyncManager.Constants;

namespace WINITSyncManager
{
    public class ProcessJobs
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public ProcessJobs(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }


        private async Task ExecutePullProcess(string entityName, PerformContext context)
        {
            context?.WriteLine($"Running {entityName} Pull Process.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var app = scope.ServiceProvider.GetRequiredService<App>();
                try
                {
                    await app.RunPullProcess(entityName, context);
                    context?.WriteLine($"{entityName} Pull Process Completed.");
                }
                catch (Exception ex)
                {
                    context?.WriteLine($"{entityName} Pull Process failed: {ex.Message}");
                    Log.Error(ex, $"Failed to execute {entityName} Pull Process.");
                    throw;
                }
            }
        }
        private async Task ExecutePushProcess(string entityName, PerformContext context)
        {
            context?.WriteLine($"Running {entityName} Push Process.");
            using (var scope = _serviceProvider.CreateScope())
            {
                var app = scope.ServiceProvider.GetRequiredService<App>();
                try
                {
                    await app.RunPushProcess(entityName, context);
                    context?.WriteLine($"{entityName} Push Process Completed.");
                }
                catch (Exception ex)
                {
                    context?.WriteLine($"{entityName} Push Process failed: {ex.Message}");
                    Log.Error(ex, $"Failed to execute {entityName} Push Process.");
                    throw;
                }
            }
        }

        public async Task RunPricingMasterPullProcessJob(PerformContext context)
       => await ExecutePullProcess(EntityNames.PricingMaster, context);

        public async Task RunPriceLadderingPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.PriceLaddering, context);

        public async Task RunTaxMasterPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.TaxMaster, context);

        public async Task RunCustomerMasterPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.CustomerMasterPull, context);

        public async Task RunCustomerCreditLimitPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.CustomerCreditLimit, context);

        public async Task RunOutstandingInvoicePullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.OutstandingInvoice, context);

        public async Task RunPurchaseOrderConfirmationPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.PurchaseOrderConfirmationPull, context);

        public async Task RunTemporaryCreditLimitPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.TemporaryCreditLimitPull, context);

        public async Task RunWarehouseStockPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.WarehouseStock, context);

        public async Task RunProvisionPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.Provision, context);

        public async Task RunProvisionCreditNotePullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.ProvisionCreditNotePull, context);
        public async Task RunItemMasterPullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.ItemMasterPull, context);
        public async Task RunInvoicePullProcessJob(PerformContext context)
            => await ExecutePullProcess(EntityNames.InvoicePull, context);
        public async Task RunPayThroughAPMasterPullProcessJob(PerformContext context)
           => await ExecutePullProcess(EntityNames.PayThroughAPMaster, context);
        public async Task RunCustomerReferencePullProcessJob(PerformContext context)
          => await ExecutePullProcess(EntityNames.CustomerReference, context);

        // push process jobs
        public async Task RunCustomerMasterPushProcessJob(PerformContext context)
            => await ExecutePushProcess(EntityNames.CustomerMasterPush, context);
        public async Task RunPurchaseOrderPushProcessJob(PerformContext context)
            => await ExecutePushProcess(EntityNames.PurchaseOrderPush, context);
        public async Task RunTemporaryCreditLimitPushProcessJob(PerformContext context)
            => await ExecutePushProcess(EntityNames.TemporaryCreditLimit, context);
        public async Task RunProvisionCreditNotePushProcessJob(PerformContext context)
            => await ExecutePushProcess(EntityNames.ProvisionCreditNotePush, context);

        public static void RegisterJobs(IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var jobInstance = new ProcessJobs(serviceProvider, configuration);

            recurringJobManager.AddOrUpdate(
                "PricingMasterPullProcess",
                () => jobInstance.RunPricingMasterPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "PriceLadderingPullProcess",
                () => jobInstance.RunPriceLadderingPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "TaxMasterPullProcess",
                () => jobInstance.RunTaxMasterPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "CustomerMasterPullProcess",
                () => jobInstance.RunCustomerMasterPullProcessJob(null),
                CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "CustomerCreditLimitPullProcess",
                () => jobInstance.RunCustomerCreditLimitPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "OutstandingInvoicePullProcess", "out-standing-invoice",
                () => jobInstance.RunOutstandingInvoicePullProcessJob(null),
                configuration["CronExpressions:OutstandingInvoicePullProcess"] == null
                ? CronExpressions.EveryThirtyMinutes
                : CommonFunctions.GetStringValue(configuration["CronExpressions:OutstandingInvoicePullProcess"]));

            recurringJobManager.AddOrUpdate(
                "PurchaseOrderConfirmationPullProcess",
                () => jobInstance.RunPurchaseOrderConfirmationPullProcessJob(null),
                 CronExpressions.EveryMinute);

            recurringJobManager.AddOrUpdate(
                "TemporaryCreditLimitPullProcess",
                () => jobInstance.RunTemporaryCreditLimitPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "WarehouseStockPullProcess",
                () => jobInstance.RunWarehouseStockPullProcessJob(null),
                 CronExpressions.EveryThreeHours);

            recurringJobManager.AddOrUpdate(
                "ProvisionPullProcess",
                () => jobInstance.RunProvisionPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "ProvisionCreditNotePullProcess",
                () => jobInstance.RunProvisionCreditNotePullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "ItemMasterPullProcess", "long-running-queue",
                () => jobInstance.RunItemMasterPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "InvoicePullProcess", "long-running-queue",
                () => jobInstance.RunInvoicePullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "PayThroughAPMasterPullProcessJob",
                () => jobInstance.RunPayThroughAPMasterPullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            recurringJobManager.AddOrUpdate(
                "CustomerReferencePullProcessJob",
                () => jobInstance.RunCustomerReferencePullProcessJob(null),
                 CronExpressions.EveryFiveMinutes);

            // push jobs
            recurringJobManager.AddOrUpdate(
                "CustomerMasterPushProcess", "push-queue",
                () => jobInstance.RunCustomerMasterPushProcessJob(null),
                 CronExpressions.EveryFiveMinutes);
            recurringJobManager.AddOrUpdate(
                "PurchaseOrderPushProcess", "push-queue",
                () => jobInstance.RunPurchaseOrderPushProcessJob(null),
                 CronExpressions.EveryMinute);
            recurringJobManager.AddOrUpdate(
                "TemporaryCreditLimitPushProcess", "push-queue",
                () => jobInstance.RunTemporaryCreditLimitPushProcessJob(null),
                 CronExpressions.EveryFiveMinutes);
            recurringJobManager.AddOrUpdate(
                "ProvisionCreditNotePushProcess", "push-queue",
                () => jobInstance.RunProvisionCreditNotePushProcessJob(null),
                 CronExpressions.EveryFiveMinutes);
        }
    }
}
