using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Promotions
{
    public class PromotionItemsModel
    {
        public string GroupTypeName { get; set; }
        public string GroupTypeUID { get; set; }
        public string GroupUID { get; set; }
        public string GroupCode { get;set; }
        public string GroupName { get; set; }

        public bool IsCompulsary { get; set; }
        public decimal Qty { get; set; }
        public Shared.Models.Enums.ActionType ActionType { get; set; }

    }
}
