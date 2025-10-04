using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISellInSchemeDTO
    {
        bool IsNew { get; set; }
        List<IWallet> Wallet { get; set; }
        ISellInSchemeHeader SellInHeader { get; set; }
        List<ISellInSchemeLine> SellInSchemeLines { get; set; }
        List<ISellInSchemeHeader> SellInSchemeHeaders { get; set; }

        public ApprovalRequestItem? ApprovalRequestItem { get; set; }
        public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }

        List<ISchemeBranch> SchemeBranches { get; set; }
        List<ISchemeBroadClassification> SchemeBroadClassifications { get; set; }
        List<ISchemeOrg> SchemeOrgs { get; set; }

    }
}
