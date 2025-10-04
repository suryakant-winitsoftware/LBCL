using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Winit.Shared.Models.Constants;
using WINITSyncManager.Constants;
using WINITSyncManager.ServicesManager;

namespace WINITSyncManager
{
    public class PullIntegration
    {
        private readonly IConfiguration _configuration;
        private readonly IPullServicesManager _pullServices;
        public PullIntegration(IConfiguration configuration, IPullServicesManager pullServices)
        {
            _configuration = configuration;
            _pullServices = pullServices;
        }
        public async Task RunPullProcessByEntity(string Entity, PerformContext context)
        {
            try
            {
                context.WriteLine(@$"Step 1:");
                context.WriteLine(@$"Start Pulling : {Entity} Data");
                context.WriteLine(@$"Prepare DB For {Entity} ");
                switch (Entity)
                {
                    case EntityNames.InvoicePull:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceHeaderPull);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceLinePull);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceSerialNoPull);
                        break;
                    case EntityNames.PurchaseOrderConfirmationPull:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderCancellation);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderStatus);
                        break;
                    default:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(Entity);
                        break;
                }
                await _pullServices.IntCommonMethods.PrepareDBByEntity(Int_EntityNames.IntegrationQueue);
                string Json = string.Empty;
                Guid guid = Guid.NewGuid();
                context.WriteLine(@$"Get Select Query From Entity table");
                string sql = await _pullServices.SyncManagerHelperMethod.GetSelectScriptByEntity(Entity);
                context.WriteLine(@$"{sql}");
                if (!string.IsNullOrEmpty(sql))
                {
                    context.WriteLine(@$" Pull {Entity} Data From Oracle ");
                    switch (Entity)
                    {
                        case EntityNames.ItemMasterPull:
                            var itemMasterData = await _pullServices.ItemMasterBL.GetItemMasterDetails(sql);
                            Json = itemMasterData.Any() ? JsonConvert.SerializeObject(itemMasterData) : string.Empty;
                            break;
                        case EntityNames.CustomerMasterPull:
                            var customers = await _pullServices.CustomerMasterPull.GetCustomerMasterPullDetails(sql);
                            Json = customers.Any() ? JsonConvert.SerializeObject(customers) : string.Empty;
                            break;

                        case EntityNames.PriceLaddering:
                            var priceLaddering = await _pullServices.PriceLaddering.GetPriceLadderingDetails(sql);
                            Json = priceLaddering.Any() ? JsonConvert.SerializeObject(priceLaddering) : string.Empty;
                            break;

                        case EntityNames.PricingMaster:
                            var pricingMaster = await _pullServices.PricingMaster.GetPricingMasterDetails(sql);
                            Json = pricingMaster.Any() ? JsonConvert.SerializeObject(pricingMaster) : string.Empty;
                            break;

                        case EntityNames.CustomerCreditLimit:
                            var customerCreditLimit = await _pullServices.CustomerCreditLimit.GetCustomerCreditLimitDetails(sql);
                            Json = customerCreditLimit.Any() ? JsonConvert.SerializeObject(customerCreditLimit) : string.Empty;
                            break;

                        case EntityNames.OutstandingInvoice:
                            var outstandingInvoice = await _pullServices.OutstandingInvoice.GetOutstandingInvoiceDetails(sql);
                            Json = outstandingInvoice.Any() ? JsonConvert.SerializeObject(outstandingInvoice) : string.Empty;
                            break;

                        case EntityNames.TaxMaster:
                            var taxMaster = await _pullServices.TaxMaster.GetTaxMasterDetails(sql);
                            Json = taxMaster.Any() ? JsonConvert.SerializeObject(taxMaster) : string.Empty;
                            break;
                        case EntityNames.InvoicePull:
                            var invoices = await _pullServices.InvoicePullBL.GetAllInvoice();
                            Json = invoices.Any() ? JsonConvert.SerializeObject(invoices) : string.Empty;
                            break;
                        case EntityNames.PurchaseOrderConfirmationPull:
                            var purchaseOrderCancellations = await _pullServices.PurchaseOrderConfirmationPull.GetPurchaseOrderConfirmationDetails();
                            Json = purchaseOrderCancellations.Any() ? JsonConvert.SerializeObject(purchaseOrderCancellations) : string.Empty;
                            break;
                        case EntityNames.StatementAndBalanceConfirmation:
                            var statementAndBalanceConfis = await _pullServices.StatementAndBalanceConfi.GetStatementAndBalanceConfiDetails(sql);
                            Json = statementAndBalanceConfis.Any() ? JsonConvert.SerializeObject(statementAndBalanceConfis) : string.Empty;
                            break;
                        case EntityNames.TemporaryCreditLimitPull:
                            var temporaryCreditLimits = await _pullServices.TemporaryCreditLimitBL.PullTemporaryCreditLimitDetailsFromOracle(sql);
                            Json = temporaryCreditLimits.Any() ? JsonConvert.SerializeObject(temporaryCreditLimits) : string.Empty;
                            break;
                        case EntityNames.WarehouseStock:
                            var warehouseStocks = await _pullServices.WarehouseStockBL.GetWarehouseStockDetails(sql);
                            Json = warehouseStocks.Any() ? JsonConvert.SerializeObject(warehouseStocks) : string.Empty;
                            break;
                        case EntityNames.Provision:
                            var provisions = await _pullServices.ProvisionBL.GetProvisionDetails(sql);
                            Json = provisions.Any() ? JsonConvert.SerializeObject(provisions) : string.Empty;
                            break;
                        case EntityNames.ProvisionCreditNotePull:
                            var provisionCreditNotes = await _pullServices.ProvisionCreditNotePull.PullProvisionCreditNoteDetailsFromOracle(sql);
                            Json = provisionCreditNotes.Any() ? JsonConvert.SerializeObject(provisionCreditNotes) : string.Empty;
                            break;
                        case EntityNames.PayThroughAPMaster:
                            var payThroughAPMasters = await _pullServices.PayThroughAPMasterBL.GetPayThroughAPMasterDetails(sql);
                            Json = payThroughAPMasters.Any() ? JsonConvert.SerializeObject(payThroughAPMasters) : string.Empty;
                            break;
                        case EntityNames.CustomerReference:
                            var customer_Refs = await _pullServices.CustomerRefBL.GetCustomerReferenceDetails(sql);
                            Json = customer_Refs.Any() ? JsonConvert.SerializeObject(customer_Refs) : string.Empty;
                            break;
                        default:
                            break;
                    }
                    //Console.WriteLine(@$" Data Pulled in Json");
                    if (!string.IsNullOrEmpty(Json))
                    {
                        context.WriteLine(@$" Data Pulled in Json");
                        SyncManagerModel.Interfaces.IApiRequest apiRequest = new SyncManagerModel.Classes.ApiRequest
                        {
                            EntityName = Entity,
                            JsonData = Json,
                            UID = guid.ToString()
                        };
                        context.WriteLine(@$"Inserting Data Into Itegration Queue Table");
                        await _pullServices.IntCommonMethods.InsertDataIntoQueue(apiRequest);
                        context.WriteLine(@$"Itegration Queue Table Insertion Completed! ");
                    }
                    else
                        context.WriteLine($@"There is no Data to Pull.");
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
