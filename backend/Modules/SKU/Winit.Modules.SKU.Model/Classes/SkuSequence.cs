using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SkuSequence : BaseModel, ISkuSequence
    {
        public string BUOrgUID { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public string SeqType { get; set; }
        public string SKUUID { get; set; }
        public int SerialNo { get; set; }
        public ActionType ActionType { get; set; }
        public string SKUCode { get; set; }
        public string SKUName { get; set; }
    }
}
