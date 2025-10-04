using Hangfire.Console;
using Hangfire.Server;
using Serilog;
using SyncManagerModel.Interfaces;
using WINITSyncManager.Constants;
using WINITSyncManager.ServicesManager;

namespace WINITSyncManager
{
    public class PushIntegration
    {
        private readonly IConfiguration _configuration;
        private readonly IPushServicesManager _pushServicesManager;
        public PushIntegration(IConfiguration configuration, IPushServicesManager pushServicesManager)
        {
            _pushServicesManager = pushServicesManager;
            _configuration = configuration;
        }
        public async Task RunPushProcess(string Entity, PerformContext context)
        {
            long syncLogId = 0;
            try
            {
                context.WriteLine($"Prepare DB By Entity.");
                //Prepare DataBase 
                switch (Entity)
                {
                    case EntityNames.PurchaseOrderPush:
                        string prepareDb = await _pushServicesManager.CommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderHeader);
                        if (!prepareDb.Equals(EntityNames.Success))
                            throw new Exception(prepareDb);
                        prepareDb = await _pushServicesManager.CommonMethods.PrepareDBByEntity(EntityNames.PurchaseOrderLine);
                        if (!prepareDb.Equals(EntityNames.Success))
                            throw new Exception(prepareDb);
                        break;
                    default:
                        prepareDb = await _pushServicesManager.CommonMethods.PrepareDBByEntity(Entity);
                        if (!prepareDb.Equals(EntityNames.Success))
                            throw new Exception(prepareDb);
                        break;
                }
                //Updating Long running process to failed
                context.WriteLine($"Updating Long running process to failed.");
                await _pushServicesManager.CommonMethods.UpdateLongRunningProcessStatusToFailure();
                // CheckCurrentRunningProcess
                context.WriteLine($"Check Current Running Process");
                int currentRunningProcessCount = await _pushServicesManager.CommonMethods.CheckCurrentRunningProcess(Entity);
                if (currentRunningProcessCount > 0)
                    throw new Exception("Another process is currently running.");
                // InitiateProcess
                context.WriteLine($"Initiate Process.");
                syncLogId = await _pushServicesManager.CommonMethods.InitiateProcess(Entity);
                if (syncLogId <= 0)
                    throw new Exception("Unable to initiate the Process.");
                context.WriteLine($"Fetch Entity Details.");
                IEntityDetails entityDetails = await _pushServicesManager.CommonMethods.FetchEntityDetails(Entity, syncLogId);

                switch (Entity)
                {
                    case EntityNames.CustomerMasterPush:
                        List<ICustomerMasterDetails> customerMasters = await _pushServicesManager.CustomerPush.GetCustomerMasterPushDetails(entityDetails);

                        //property names to exclude from validation
                        var excludedProperties = new HashSet<string> { "Address2", "Mobile", "OracleCustomerCode", "OracleLocationCode", "ReadFromOracle" };
                        //Filtering out objects with null properties.
                        List<ICustomerMasterDetails> validObjects = customerMasters.Where(obj => obj.GetType().GetProperties()
                            .Where(Pi => !excludedProperties
                            .Contains(Pi.Name))
                            .Any(pi => pi.GetValue(obj) != null &&
                                !(pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) &&
                                !(pi.GetValue(obj) is int intVal && intVal == 0) &&
                                !(pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime))))
                            .ToList();

                        List<ICustomerMasterDetails> missingObjects = customerMasters.Except(validObjects).ToList();
                        missingObjects
                            .ForEach(obj => obj.ErrorDescription = string.Join(",", obj.GetType().GetProperties()
                            .Where(pi => pi.GetValue(obj) == null ||
                               (pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) ||
                               (pi.GetValue(obj) is int intVal && intVal == 0) ||
                               (pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime)))
                            .Select(pi => pi.Name).ToList()));

                        List<IPushDataStatus> pushDataStatus = missingObjects.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.StoreUID,
                            LinkedItemType = EntityNames.CustomerMasterPushType,
                            ErrorMessage = obj.ErrorDescription,
                            Status = EntityNames.Fail
                        }).ToList().Cast<IPushDataStatus>().ToList();

                        pushDataStatus.AddRange(validObjects.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.StoreUID,
                            LinkedItemType = EntityNames.CustomerMasterPushType,
                            ErrorMessage = EntityNames.Success,
                            Status = EntityNames.Success
                        }).ToList().Cast<IPushDataStatus>().ToList());
                        pushDataStatus.AddRange(validObjects.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.AddressKey,
                            LinkedItemType = EntityNames.CustomerMasterPushType,
                            ErrorMessage = EntityNames.Success,
                            Status = EntityNames.Success
                        }).ToList().Cast<IPushDataStatus>().ToList());

                        //var distinctStoreUID = customerMasters.Select(x => x.StoreUID).Distinct().ToList();
                        validObjects.ForEach(obj => obj.InsertedOn = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                        await _pushServicesManager.CustomerPush.InsertCustomerdetailsIntoOracleStaging(validObjects);
                        //await _int_CommonMethods.UpdatePushDataStatusByUID(distinctStoreUID);
                        await _pushServicesManager.CommonMethods.UpdatePushDataStatusByUID(pushDataStatus);
                        await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, EntityNames.Success, 200);
                        break;
                    case EntityNames.PurchaseOrderPush:

                        List<Iint_PurchaseOrderHeader> purchaseOrderHeaders = await _pushServicesManager.PurchaseOrder.GetPurchaseOrderHeaderDetails(entityDetails);
                        List<Iint_PurchaseOrderLine> purchaseOrderLines = await _pushServicesManager.PurchaseOrder.GetPurchaseOrderLineDetails(entityDetails);
                        //var distinctStoreUID = purchaseOrderHeaders.Select(x => x.PurchaseOrderUid).Distinct().ToList();

                        //property names to exclude from validation
                        var excludedPropertiesPO = new HashSet<string> { "UID", "OrderType", "SalesOffice", "OperatingUnit", "SalesPerson" };
                        //Filtering out objects with null properties.
                        /*Purchase Order Header*/
                        List<Iint_PurchaseOrderHeader> validHeaderObjects = purchaseOrderHeaders.Where(obj => obj.GetType().GetProperties()
                        .Where(Pi => !excludedPropertiesPO.Contains(Pi.Name))
                        .Any(pi => pi.GetValue(obj) != null &&
                            !(pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) &&
                            !(pi.GetValue(obj) is int intVal && intVal == 0) &&
                            !(pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime))))
                        .ToList();

                        List<Iint_PurchaseOrderHeader> missingHeaderObjects = purchaseOrderHeaders.Except(validHeaderObjects).ToList();
                        missingHeaderObjects
                            .ForEach(obj => obj.ErrorDescription = string.Join(",", obj
                            .GetType()
                            .GetProperties()
                            .Where(pi => pi.GetValue(obj) == null ||
                                 (pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) ||
                                 (pi.GetValue(obj) is int intVal && intVal == 0) ||
                                 (pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime)))
                            .Select(pi => pi.Name).ToList()));
                        /*Purchase Order Line*/
                        List<Iint_PurchaseOrderLine> validLineObjects = purchaseOrderLines.Where(obj => obj.GetType().GetProperties()
                            .Where(Pi => !excludedPropertiesPO
                            .Contains(Pi.Name))
                            .Any(pi => pi.GetValue(obj) != null &&
                                !(pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) &&
                                !(pi.GetValue(obj) is int intVal && intVal == 0) &&
                                !(pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime))))
                            .ToList();

                        List<Iint_PurchaseOrderLine> missingLineObjects = purchaseOrderLines.Except(validLineObjects).ToList();

                        missingHeaderObjects.AddRange(
                            purchaseOrderHeaders
                            .Where(header => missingLineObjects
                            .Select(line => line.PurchaseOrderUid).Distinct().ToList()
                            .Contains(header.PurchaseOrderUid) && !missingHeaderObjects
                            .Contains(header))
                            .ToList());

                        missingHeaderObjects.ForEach(obj =>
                        obj.ErrorDescription = string.Join(
                            ",", obj.GetType().GetProperties()
                            .Where(pi => pi.GetValue(obj) == null ||
                                (pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) ||
                                (pi.GetValue(obj) is int intVal && intVal == 0) ||
                                (pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime)))
                            .Select(pi => pi.Name).ToList()));

                        missingLineObjects.ForEach(obj =>
                            obj.ErrorDescription = string.Join(
                                ",",
                                obj.GetType().GetProperties()
                                    .Where(pi =>
                                        pi.GetValue(obj) == null ||
                                        (pi.GetValue(obj) is string str && string.IsNullOrWhiteSpace(str)) ||
                                        (pi.GetValue(obj) is int intVal && intVal == 0) ||
                                        (pi.GetValue(obj) is DateTime dateTime && dateTime == default(DateTime))
                                    )
                                    .Select(pi => pi.Name)
                                    .ToList()
                            )
                        );

                        missingHeaderObjects.ForEach(obj => obj.ErrorDescription = obj.ErrorDescription +
                        string.Join(",Lines: ", missingLineObjects.Where(lineobj => lineobj.PurchaseOrderUid == obj.PurchaseOrderUid)
                        .Select(line => line.ErrorDescription)));

                        purchaseOrderHeaders = purchaseOrderHeaders.Where(obj => !missingHeaderObjects.Contains(obj)).ToList();
                        purchaseOrderLines = purchaseOrderLines.Where(obj => !missingLineObjects.Contains(obj)
                        && !missingHeaderObjects.Select(header => header.PurchaseOrderUid).Distinct().ToList()
                        .Contains(obj.PurchaseOrderUid)).ToList();

                        List<IPushDataStatus> pushDataStatusPO = missingHeaderObjects.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.PurchaseOrderUid,
                            LinkedItemType = EntityNames.PurchaseOrderPushType,
                            ErrorMessage = obj.ErrorDescription,
                            Status = EntityNames.Fail
                        }).ToList().Cast<IPushDataStatus>().ToList();
                        pushDataStatusPO.AddRange(purchaseOrderHeaders.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.PurchaseOrderUid,
                            LinkedItemType = EntityNames.PurchaseOrderPushType,
                            ErrorMessage = EntityNames.Success,
                            Status = EntityNames.Success
                        }).ToList().Cast<IPushDataStatus>().ToList());
                        //Push data status
                        purchaseOrderHeaders.ForEach(obj => obj.InsertedOn = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                        await _pushServicesManager.PurchaseOrder.InsertPurchaseOrderIntoOraceleStaging(purchaseOrderHeaders, purchaseOrderLines);
                        //await _pushServicesManager.CommonMethods.UpdatePushDataStatusByUID(distinctStoreUID);
                        await _pushServicesManager.CommonMethods.UpdatePushDataStatusByUID(pushDataStatusPO);
                        await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, EntityNames.Success, 200);
                        break;
                    case EntityNames.TemporaryCreditLimit:
                        List<ITemporaryCreditLimit> temporaryCreditLimits = await _pushServicesManager.TemporaryCreditLimit.GetTemporaryCreditLimitPushDetails(entityDetails);
                        temporaryCreditLimits.ForEach(obj => obj.InsertedOn = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                        await _pushServicesManager.TemporaryCreditLimit.InsertTemporaryCreditLimitsIntoOracleStaging(temporaryCreditLimits);
                        List<IPushDataStatus> temporaryCreditLimitPushStatus = temporaryCreditLimits.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.ReqUID,
                            LinkedItemType = EntityNames.TemporaryCreditLimit,
                            ErrorMessage = obj.ErrorDescription,
                            Status = EntityNames.Success
                        }).ToList().Cast<IPushDataStatus>().ToList();
                        await _pushServicesManager.CommonMethods.UpdatePushDataStatusByUID(temporaryCreditLimitPushStatus);
                        await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, EntityNames.Success, 200);
                        break;
                    case EntityNames.ProvisionCreditNotePush:
                        List<IProvisionCreditNote> provisionCredits = await _pushServicesManager.ProvisionCreditNotePush.GetCreditNoteProvisionDetails(entityDetails);
                        provisionCredits.ForEach(obj => obj.InsertedOn = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
                        await _pushServicesManager.ProvisionCreditNotePush.InsertCreditNoteProvisionsIntoOracleStaging(provisionCredits);

                        List<IPushDataStatus> provisionCreditsPushStatus = provisionCredits.Select(obj => new SyncManagerModel.Classes.PushDataStatus
                        {
                            LinkedItemUid = obj.UID,
                            LinkedItemType = EntityNames.ProvisionCreditNotePush,
                            ErrorMessage = obj.ErrorDescription,
                            Status = EntityNames.Success,
                        }).ToList().Cast<IPushDataStatus>().ToList();
                        await _pushServicesManager.CommonMethods.UpdatePushDataStatusByUID(provisionCreditsPushStatus);
                        await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, EntityNames.Success, 200);
                        break;
                    default:
                        await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, EntityNames.Fail, -100);
                        break;
                }

            }
            catch (Exception ex)
            {
                await _pushServicesManager.CommonMethods.ProcessCompletion(syncLogId, ex.Message, -100);
                context.WriteLine($"The PushProcess failed {ex.Message}");
                Log.Error(ex, "The PushProcess failed to run.");
                throw;
            }
        }
    }
}
