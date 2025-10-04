using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.ListHeader.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;


namespace Winit.Modules.Promotion.BL.UIInterfaces
{
    public interface IPromotionBase : ITableGridViewModel
    {
        bool IsLoad { get; set; }
        List<ListItem> PromotionsDropDowns { get; set; }
        List<Winit.Modules.Promotion.Model.Classes.Promotion> PromotionsList { get; set; }

        Task PageLoadFieldsOfMaintainPromotion();
    }
}
