using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;

namespace Winit.Modules.SKU.Model.UIInterfaces;

public interface ISKUGroupItemView:ISKUGroup
{
    public string ParentSKUGroupTypeUID { get; set; }
    public bool IsCreatePopUpOpen { get; set; }
    public bool IsUpdatePopUpOpen { get; set; }
    public bool IsDeletePopUpOpen { get; set; }
    public bool IsOpen { get; set; }
    public string SupplierName { get; set; }
    public string SKUGroupTypeName { get; set; }
    public string SKUGroupTypeCode{ get; set; }
    public bool HasChild { get; set; }
    public List<ISKUGroupItemView>? ChildGrids { get; set; }
}
