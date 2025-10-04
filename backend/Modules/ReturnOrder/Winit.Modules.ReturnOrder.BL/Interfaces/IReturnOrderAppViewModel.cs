using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnOrderAppViewModel : IReturnOrderViewModel
{
    public Dictionary<ISelectionItem, List<ISelectionItem>> FilterDataList { get; set; }
    bool FilterAction(List<FilterCriteria> filterCriterias, IReturnOrderItemView itemView);
}
