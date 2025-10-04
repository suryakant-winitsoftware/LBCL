using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.UIModels.Common.Filter
{
    public class FilterModel
    {
        public string ColumnName { get; set; }
        public string Label { get; set; }
        public bool HasChildDependency { get; set; }
        public List<ISelectionItem> DropDownValues { get; set; } = new List<ISelectionItem>();
        public bool IsCodeOnDDLSelect { get; set; }
        public string FilterType { get; set; }
        public SelectionMode SelectionMode { get; set; }
        public string SelectedValue { get; set; }
        public string? PlaceHolder { get; set; } = "Enter Text..";
        public string? Class { get; set; }
        public string? Style { get; set; }
        public bool IsForSearch { get; set; }
        public int Width { get; set; } = 250;
        public bool SelectedBoolValue { get; set; }
        public bool IsDefaultValueNeeded { get; set; }
        public string DefaultValue { get; set; }
        public List<ISelectionItem> SelectedValues { get; set; }

        public Dictionary<string, List<string>> filterValues = new Dictionary<string, List<string>>();
        public bool IsDependent { get; set; }

        //public delegate Task OnDropDownSelect(DropDownEvent dropDownEvent);
        public Func<DropDownEvent, Task> OnDropDownSelect { get; set; }
        public FilterModel()
        {
            SelectedValue = "";
        }
    }
}
