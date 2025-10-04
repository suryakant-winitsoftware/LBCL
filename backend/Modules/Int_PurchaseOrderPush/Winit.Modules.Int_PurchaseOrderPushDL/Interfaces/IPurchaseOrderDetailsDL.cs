using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using Winit.Modules.Int_PurchaseOrderPushModel.Interfaces;

namespace Winit.Modules.Int_PurchaseOrderPushDL.Interfaces
{
    public interface IPurchaseOrderDetailsDL
    {
        Task<List<Iint_PurchaseOrderHeader>> GetPurchaseOrderHeaderDetails(IEntityDetails entityDetails)
        {
            throw new NotImplementedException();
        } 
        Task<List<Iint_PurchaseOrderLine>> GetPurchaseOrderLineDetails(IEntityDetails entityDetails)
        {
            throw new NotImplementedException();
        }
        Task<string> InsertPurchaseOrderIntoOraceleStaging(List<Iint_PurchaseOrderHeader> purchaseOrderHeaders, List<Iint_PurchaseOrderLine> purchaseOrderLines)
        {
            throw new NotImplementedException();
        }
    }
}
