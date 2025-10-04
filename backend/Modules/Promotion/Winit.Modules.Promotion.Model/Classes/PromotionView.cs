using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class PromotionView : Promotion
    {
        public bool ApplyForExcludedItems { get; set; }
        public ActionType ActionType { get; set; }

        public string EndDateRemarks { get; set; }
        public string EndDateUpdatedByEmpUID { get; set; }
        public DateTime EndDateUpdatedOn { get; set; }
        public bool HasHistory {  get; set; }
        public ISchemeExtendHistory SchemeExtendHistory { get; set; }

    }
}
