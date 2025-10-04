using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WinIt.Models;

public class MappingComponentDTO
{
    public List<ISelectionItem> TabSelectionItems { get; set; }
    public int TabIndex { get; set; }
    public bool IsExcluded { get; set; }
    public List<ISelectionItem> LocationTypesSelectionItems { get; set; }
    public List<ISelectionItem> LocationsSelectionItems { get; set; }
    public List<ISelectionItem> StoreGroupTypesSelectionItems { get; set; }
    public List<ISelectionItem> StoreGroupsSelectionItems { get; set; }
    public List<DataGridColumn> DataGridColumns { get; set; }
    public List<IMappingItemView> GridDataSource { get; set; }
} 