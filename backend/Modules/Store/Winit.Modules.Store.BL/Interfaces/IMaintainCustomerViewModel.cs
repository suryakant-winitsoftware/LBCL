using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IMaintainCustomerViewModel : ITableGridViewModel
    {
        #region Maintain Page

        #endregion
        List<IStore> Stores { get; }
        List<ISelectionItem> PriceTypeSelectionItems { get; }
    }
}
