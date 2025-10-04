using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnSummaryWebViewModel:IReturnSummaryViewModel
{
    public List<ISelectionItem> RouteSelectionItems { get; set; }
    public List<ISelectionItem> StoreSelectionItems { get; set; }
}
