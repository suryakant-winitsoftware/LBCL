using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;
using Winit.Shared.Models.Constants;

namespace SyncManagerBL.Classes
{
    public class Int_PurchaseOrderConfirmationBL : Iint_PurchaseOrderConfirmationBL
    {
        private readonly Iint_PurchaseOrderConfirmationDL _purchaseOrderConfirmationDL;
        private readonly Iint_PurchaseOrderConfirmationStagingDL _PurchaseOrderConfirmationStagingDL;
        private readonly IEntityScriptBL _entityScriptBL;
        public Int_PurchaseOrderConfirmationBL(Iint_PurchaseOrderConfirmationDL purchaseOrderConfirmationDL,
            Iint_PurchaseOrderConfirmationStagingDL purchaseOrderConfirmationStaging,
            IEntityScriptBL entityScriptBL)
        {
            _purchaseOrderConfirmationDL = purchaseOrderConfirmationDL;
            _PurchaseOrderConfirmationStagingDL = purchaseOrderConfirmationStaging;
            _entityScriptBL = entityScriptBL;
        }
        public async Task<List<Iint_PurchaseOrderCancellation>> GetPurchaseOrderCancellationDetails(string sql)
        {
            return await _purchaseOrderConfirmationDL.GetPurchaseOrderCancellationDetails(sql);
        }

        public async Task<List<Iint_PurchaseOrderStatus>> GetPurchaseOrderConfirmationDetails()
        {
            try
            {

                IEntityScript poStatusEntityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Int_EntityNames.PurchaseOrderStatus);
                IEntityScript poCancellationEntityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Int_EntityNames.PurchaseOrderCancellation);

                bool areAllScriptsEmpty = poStatusEntityScript == null || poCancellationEntityScript == null;
                if (areAllScriptsEmpty)
                    throw new Exception(@$"select query is empty for {Int_EntityNames.PurchaseOrderStatus} or {Int_EntityNames.PurchaseOrderCancellation} ");
                List<Iint_PurchaseOrderStatus> purchaseOrderStatuses = await GetPurchaseOrderStatusDetails(poStatusEntityScript.SelectQuery + $" FETCH FIRST {(poStatusEntityScript.MaxCount == 0 ? 10 : poStatusEntityScript.MaxCount)} ROWS ONLY ");
                List<Iint_PurchaseOrderCancellation> purchaseOrderCancellations = await GetPurchaseOrderCancellationDetails(poCancellationEntityScript.SelectQuery + $" FETCH FIRST {(poCancellationEntityScript.MaxCount == 0 ? 10 : poCancellationEntityScript.MaxCount)} ROWS ONLY ");
                var poCancellationsByOrderNumber = purchaseOrderCancellations.GroupBy(l => l.ErpOrderNumber).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var purchaseOrderStatuse in purchaseOrderStatuses)
                {
                    if (poCancellationsByOrderNumber.TryGetValue(purchaseOrderStatuse.ErpOrderNumber??"", out var Cancellations))
                    {
                        purchaseOrderStatuse.iint_PurchaseOrderCancellations = Cancellations;
                    }
                }
                return purchaseOrderStatuses ==null ? new List<Iint_PurchaseOrderStatus>() : purchaseOrderStatuses;
            }
            catch { throw; }

        }

        public async Task<List<Iint_PurchaseOrderStatus>> GetPurchaseOrderStatusDetails(string sql)
        {
            return await _purchaseOrderConfirmationDL.GetPurchaseOrderStatusDetails(sql);
        }

        public async Task<int> InsertPurchaseOrderCancellationDataIntoMonthTable(List<Iint_PurchaseOrderCancellation> purchaseOrderCancellations, IEntityDetails entityDetails)
        {
            return await _PurchaseOrderConfirmationStagingDL.InsertPurchaseOrderCancellationDataIntoMonthTable(purchaseOrderCancellations, entityDetails);
        }

        public async Task<int> InsertPurchaseOrderStatusDataIntoMonthTable(List<Iint_PurchaseOrderStatus> purchaseOrderStatuses, IEntityDetails entityDetails)
        {
            return await _PurchaseOrderConfirmationStagingDL.InsertPurchaseOrderStatusDataIntoMonthTable(purchaseOrderStatuses, entityDetails);
        }
    }
}
