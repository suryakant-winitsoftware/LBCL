using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ApprovalEngine.Model.Classes;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SalesPromotionSchemeApprovalDTO
    {
        public Interfaces.ISalesPromotionScheme SalesPromotion { get; set; }
        public ApprovalRequestItem? ApprovalRequestItem { get; set; }
        public ApprovalStatusUpdate? ApprovalStatusUpdate { get; set; }
    }
}
