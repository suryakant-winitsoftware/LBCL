using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.Model.UIClasses;

public class SKUAttributeDropdownModel
{
    public string UID { get; set; } = string.Empty;
    public string DropDownTitle { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ParentUID { get; set; }
    public List<ISelectionItem> DropDownDataSource { get; set; } = new List<ISelectionItem>();
}
