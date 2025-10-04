using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Scheme.Model.Constants
{
    public class SalesPromotionConstants
    {
        public static List<ISelectionItem> ActivityType=new List<ISelectionItem>()
        {
            new SelectionItem(){UID="DD",Code="DisplayDiscount",Label="Display Discount" },
            new SelectionItem(){UID="P",Code="Print",Label="Print" },
            new SelectionItem(){UID="KSPA",Code="KanopySalesPromotionActivity",Label="Kanopy Sales Promotion Activity" },
        };
    }
}
