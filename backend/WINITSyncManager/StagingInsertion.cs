using Hangfire.Console;
using Hangfire.Server;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SyncManagerModel.Classes;
using SyncManagerModel.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Constants;
using WINITSyncManager.Constants;
using WINITSyncManager.ServicesManager;

namespace WINITSyncManager
{
    public class StagingInsertion
    {
        private readonly IPullServicesManager _pullServices;
        public StagingInsertion(IPullServicesManager pullServices)
        {
            _pullServices = pullServices;
        }
        public async Task RunStagingInsertionProcessByEntity(string Entity, PerformContext context)
        {
            long syncLogId = 0;
            try
            {
                context.WriteLine(@$"Step 2:");
                context.WriteLine(@$"Insert {Entity} Data Into Staging Database");
                //Prepare DataBase
                switch (Entity)
                {
                    case EntityNames.InvoicePull:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceHeaderPull);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceLinePull);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.InvoiceSerialNoPull);
                        break;
                    case EntityNames.PurchaseOrderConfirmationPull:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderStatus);
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderCancellation);
                        break;
                    default:
                        await _pullServices.IntCommonMethods.PrepareDBByEntity(Entity);
                        break;
                }
                List<IApiRequest> data = await _pullServices.IntCommonMethods.GetDataFromQueue(Entity);
                if (data.Count < 0)
                    throw new Exception("No records left to process!");
                foreach (IApiRequest apiRequest in data)
                {
                    context.WriteLine(@$"Initiate Process");
                    syncLogId = await _pullServices.IntCommonMethods.InitiateProcess(Entity);
                    if (syncLogId <= 0)
                        throw new Exception("Unable to initiate the Process.");
                    IEntityDetails entityDetails = await _pullServices.IntCommonMethods.FetchEntityDetails(Entity, syncLogId);
                    if (entityDetails is null || entityDetails?.TableName is null)
                        throw new Exception("Failed to get Entity Details or Table Name.");
                    await InsertIntoMonthTable(Entity, apiRequest, entityDetails);
                    await _pullServices.IntCommonMethods.ProcessCompletion(syncLogId, EntityNames.Success, 200);
                    context.WriteLine(@$"Staging Insertion Process Completed.");
                }
            }
            catch (Exception ex)
            {
                await _pullServices.IntCommonMethods.ProcessCompletion(syncLogId, ex.Message, -100);
                context.WriteLine(@$"Staging Insertion Process Failed. error: {ex.Message}");
                throw;
            }
        }
        private async Task InsertIntoMonthTable(string Entity, IApiRequest Data, IEntityDetails entityDetails)
        {
            try
            {
                if (Data.JsonData.IsNullOrEmpty())
                    throw new Exception("The JSON data does not contain any information.");
                Entity = Entity.IsNullOrEmpty() ? CommonFunctions.GetStringValue(Data.EntityName) : Entity;
                switch (Entity)
                {
                    case EntityNames.ItemMasterPull:
                        List<SyncManagerModel.Classes.ItemMaster>? items = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.ItemMaster>>(Data?.JsonData);
                        List<IItemMaster> iitems = items.Cast<IItemMaster>().ToList();
                        _ = await _pullServices.ItemMasterBL.InsertItemDataIntoMonthTable(iitems, entityDetails);
                        break;
                    case EntityNames.PricingMaster:
                        List<SyncManagerModel.Classes.PricingMaster>? Prices = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.PricingMaster>>(Data?.JsonData);
                        List<IPricingMaster> iPrices = Prices.Cast<IPricingMaster>().ToList();
                        await _pullServices.PricingMaster.InsertPricingDataIntoMonthTable(iPrices, entityDetails);
                        break;
                    case EntityNames.PriceLaddering:
                        List<SyncManagerModel.Classes.PriceLaddering>? priceLadderings = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.PriceLaddering>>(Data?.JsonData);
                        List<IPriceLaddering> ipriceLadderings = priceLadderings.Cast<IPriceLaddering>().ToList();
                        await _pullServices.PriceLaddering.InsertPriceLadderingDataIntoMonthTable(ipriceLadderings, entityDetails);
                        break;
                    case EntityNames.TaxMaster:
                        List<SyncManagerModel.Classes.TaxMaster>? taxMasters = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.TaxMaster>>(Data?.JsonData);
                        List<ITaxMaster> itaxMasters = taxMasters.Cast<ITaxMaster>()
                            .ToList();
                        await _pullServices.TaxMaster.InsertTaxDataIntoMonthTable(itaxMasters, entityDetails);
                        break;
                    case EntityNames.CustomerCreditLimit:
                        List<SyncManagerModel.Classes.CustomerCreditLimit>? creditLimits = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.CustomerCreditLimit>>(Data?.JsonData);
                        List<ICustomerCreditLimit> icreditLimits = creditLimits.Cast<ICustomerCreditLimit>()
                            .ToList();
                        await _pullServices.CustomerCreditLimit.InsertCreditLimitDataIntoMonthTable(icreditLimits, entityDetails);
                        break;
                    case EntityNames.CustomerMasterPull:
                        List<SyncManagerModel.Classes.CustomerMasterPull>? customerMasters = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.CustomerMasterPull>>(Data?.JsonData);
                        List<ICustomerMasterPull> icustomerMasters = customerMasters.Cast<ICustomerMasterPull>()
                            .ToList();
                        await _pullServices.CustomerMasterPull.InsertCustomerMasterPullDataIntoMonthTable(icustomerMasters, entityDetails);
                        break;
                    case EntityNames.InvoicePull:
                        List<SyncManagerModel.Classes.Int_InvoiceHeader>? invoiceHeaders = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.Int_InvoiceHeader>>(Data?.JsonData);
                        List<IInt_InvoiceHeader> iinvoiceHeaders = invoiceHeaders.Cast<IInt_InvoiceHeader>().ToList();
                        entityDetails.TableName = Int_DbTableName.InvoiceHeader + Int_DbTableName.MonthTableSuffix;
                        await _pullServices.InvoicePullBL.InsertInvoiceHeaderDataIntoMonthTable(iinvoiceHeaders, entityDetails);
                        entityDetails.TableName = Int_DbTableName.InvoiceLine + Int_DbTableName.MonthTableSuffix;
                        await _pullServices.InvoicePullBL.InsertInvoiceLineDataIntoMonthTable(iinvoiceHeaders.SelectMany(header => header.InvoiceLines).ToList(), entityDetails);
                        entityDetails.TableName = Int_DbTableName.InvoiceSerialNo + Int_DbTableName.MonthTableSuffix;
                        await _pullServices.InvoicePullBL.InsertInvoiceSerialNoDataIntoMonthTable(iinvoiceHeaders.SelectMany(header => header.InvoiceSerialNos).ToList(), entityDetails);
                        entityDetails.TableName = Int_DbTableName.InvoiceHeader + Int_DbTableName.MonthTableSuffix;
                        break;
                    case EntityNames.StatementAndBalanceConfirmation:
                        List<SyncManagerModel.Classes.StatementAndBalanceConfi>? statementAndBalances = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.StatementAndBalanceConfi>>(Data?.JsonData);
                        List<IStatementAndBalanceConfi> istatementAndBalances = statementAndBalances.Cast<IStatementAndBalanceConfi>().ToList();
                        await _pullServices.StatementAndBalanceConfi.InsertStatementAndBalanceConfiIntoMonthTable(istatementAndBalances, entityDetails);
                        break;
                    case EntityNames.PurchaseOrderConfirmationPull:
                        List<SyncManagerModel.Classes.Int_PurchaseOrderStatus>? purchaseOrderStatuses = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.Int_PurchaseOrderStatus>>(Data?.JsonData);
                        List<Iint_PurchaseOrderStatus> ipurchaseOrderStatuses = purchaseOrderStatuses.Cast<Iint_PurchaseOrderStatus>().ToList();
                        entityDetails.TableName = Int_DbTableName.PurchaseOrderStatus + Int_DbTableName.MonthTableSuffix;
                        await _pullServices.PurchaseOrderConfirmationPull.InsertPurchaseOrderStatusDataIntoMonthTable(ipurchaseOrderStatuses, entityDetails);
                        entityDetails.TableName = Int_DbTableName.PurchaseOrderCancellation + Int_DbTableName.MonthTableSuffix;
                        await _pullServices.PurchaseOrderConfirmationPull.InsertPurchaseOrderCancellationDataIntoMonthTable(ipurchaseOrderStatuses.SelectMany(obj => obj.iint_PurchaseOrderCancellations).ToList(), entityDetails);
                        entityDetails.TableName = Int_DbTableName.PurchaseOrderStatus + Int_DbTableName.MonthTableSuffix;
                        break;
                    case EntityNames.OutstandingInvoice:
                        List<SyncManagerModel.Classes.OutstandingInvoice>? outstandings = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.OutstandingInvoice>>(Data?.JsonData);
                        List<IOutstandingInvoice> ioutstandings = outstandings.Cast<IOutstandingInvoice>()
                            .ToList();
                        await _pullServices.OutstandingInvoice.InsertOutstandingInvoiceDataIntoMonthTable(ioutstandings, entityDetails);
                        break;
                    case EntityNames.TemporaryCreditLimitPull:
                        List<SyncManagerModel.Classes.TemporaryCreditLimit>? temporaryCreditLimits = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.TemporaryCreditLimit>>(Data?.JsonData);
                        List<ITemporaryCreditLimit> itemporaryCreditLimits = temporaryCreditLimits.Cast<ITemporaryCreditLimit>()
                            .ToList();
                        await _pullServices.TemporaryCreditLimitBL.InsertTemporaryCreditLimitDetailsIntoMonthTable(itemporaryCreditLimits, entityDetails);
                        break;
                    case EntityNames.WarehouseStock:
                        List<SyncManagerModel.Classes.WarehouseStock>? warehouseStocks = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.WarehouseStock>>(Data?.JsonData);
                        List<IWarehouseStock> iwarehouseStocks = warehouseStocks.Cast<IWarehouseStock>()
                            .ToList();
                        await _pullServices.WarehouseStockBL.InsertWarehouseStockDataIntoMonthTable(iwarehouseStocks, entityDetails);
                        break;
                    case EntityNames.Provision:
                        List<SyncManagerModel.Classes.Provision>? provisions = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.Provision>>(Data?.JsonData);
                        List<IProvision> iprovisions = provisions.Cast<IProvision>()
                            .ToList();
                        await _pullServices.ProvisionBL.InsertProvisionDataIntoMonthTable(iprovisions, entityDetails);
                        break;
                    case EntityNames.ProvisionCreditNotePull:
                        List<SyncManagerModel.Classes.ProvisionCreditNote>? provisionCreditNotes = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.ProvisionCreditNote>>(Data?.JsonData);
                        List<IProvisionCreditNote> iprovisionCreditNotes = provisionCreditNotes.Cast<IProvisionCreditNote>()
                            .ToList();
                        await _pullServices.ProvisionCreditNotePull.InsertProvisionCreditNoteDetailsIntoMonthTable(iprovisionCreditNotes, entityDetails);
                        break;
                    case EntityNames.PayThroughAPMaster:
                        List<SyncManagerModel.Classes.PayThroughAPMaster>? payThroughAPMasters = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.PayThroughAPMaster>>(Data?.JsonData);
                        List<IPayThroughAPMaster> ipayThroughAPMasters = payThroughAPMasters.Cast<IPayThroughAPMaster>()
                            .ToList();
                        await _pullServices.PayThroughAPMasterBL.InsertPayThroughAPMasterDataIntoMonthTable(ipayThroughAPMasters, entityDetails);
                        break;
                    case EntityNames.CustomerReference:
                        List<SyncManagerModel.Classes.Customer_Ref>? customer_Refs = JsonConvert.DeserializeObject<List<SyncManagerModel.Classes.Customer_Ref>>(Data?.JsonData);
                        List<ICustomer_Ref> icustomer_Refs = customer_Refs.Cast<ICustomer_Ref>()
                            .ToList();
                        await _pullServices.CustomerRefBL.InsertCustomerReferenceDataIntoMonthTable(icustomer_Refs, entityDetails);
                        break;
                    default:
                        break;
                }
                IIntegrationMessageProcess integrationProcess = new IntegrationMessageProcess()
                {
                    SyncLogDetailId = entityDetails.SyncLogDetailId,
                    InterfaceName = Entity,
                    MonthTableName = entityDetails.TableName,
                    ProcessStatus = 0,
                    TablePrefix = entityDetails.TablePrefix
                };
                await _pullServices.IntCommonMethods.IntegrationProcessStatusInsertion(integrationProcess);
                _ = await _pullServices.IntCommonMethods.UpdateQueueStatus(Data?.UID);

            }
            catch { throw; }
        }

    }
}
