using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_PurchaseOrderPushBL.Interfaces;
using Winit.Modules.Int_PurchaseOrderPushDL.Interfaces;
using Winit.Modules.Int_PurchaseOrderPushModel.Interfaces;
using Winit.Shared.Models.Constants;

namespace Winit.Modules.Int_PurchaseOrderPushBL.Classes
{
    public class PurchaseOrderDetailsBL : IPurchaseOrderDetailsBL
    {
        private readonly IPurchaseOrderDetailsDL _mssqlPurchaseOrder;
        private readonly IPurchaseOrderDetailsDL _oraclePurchaseOrder;
        public PurchaseOrderDetailsBL(Func<string, IPurchaseOrderDetailsDL> purchaseOrderDetailsDL)
        {
            _oraclePurchaseOrder = purchaseOrderDetailsDL(ConnectionStringName.OracleServer);
            _mssqlPurchaseOrder = purchaseOrderDetailsDL(ConnectionStringName.SqlServer);
        }

        public async Task<List<Iint_PurchaseOrderHeader>> GetPurchaseOrderHeaderDetails(IEntityDetails entityDetails)
        {
            return await _mssqlPurchaseOrder.GetPurchaseOrderHeaderDetails(entityDetails);
        }

        public async Task<List<Iint_PurchaseOrderLine>> GetPurchaseOrderLineDetails(IEntityDetails entityDetails)
        {
            return await _mssqlPurchaseOrder.GetPurchaseOrderLineDetails(entityDetails);
        }

        public async Task<string> InsertPurchaseOrderIntoOraceleStaging(List<Iint_PurchaseOrderHeader> purchaseOrderHeaders, List<Iint_PurchaseOrderLine> purchaseOrderLines)
        {
            return await _oraclePurchaseOrder.InsertPurchaseOrderIntoOraceleStaging(purchaseOrderHeaders, purchaseOrderLines);
        }
        
    }
}
