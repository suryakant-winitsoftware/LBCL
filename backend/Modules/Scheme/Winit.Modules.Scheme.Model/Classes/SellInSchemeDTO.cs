using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SellInSchemeDTO : ISellInSchemeDTO
    {
        public bool IsNew { get; set; }
        public List<IWallet> Wallet { get; set; }
        public ISellInSchemeHeader SellInHeader { get; set; }
        public List<ISellInSchemeLine> SellInSchemeLines { get; set; }
        public List<ISellInSchemeHeader> SellInSchemeHeaders { get; set; }

        public ApprovalRequestItem? ApprovalRequestItem { get; set; }
        public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }

        public List<ISchemeBranch> SchemeBranches { get; set; }
        public List<ISchemeBroadClassification> SchemeBroadClassifications { get; set; }
        public List<ISchemeOrg> SchemeOrgs { get; set; }

    }
}
