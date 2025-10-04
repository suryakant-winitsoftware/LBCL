using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.UIInterfaces;

namespace Winit.Modules.SKU.Model.UIClasses;

public class SKUGroupItemView:SKUGroup,ISKUGroupItemView
{
    public string ParentSKUGroupTypeUID { get; set; }
    public bool IsCreatePopUpOpen { get; set; }
    public bool IsUpdatePopUpOpen { get; set; }
    public bool IsDeletePopUpOpen { get; set; }
    public bool IsOpen { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SKUGroupTypeName { get; set; } = string.Empty;
    public string SKUGroupTypeCode { get; set; } = string.Empty;
    public bool HasChild { get; set; }
    public List<ISKUGroupItemView>? ChildGrids { get; set; }
}
