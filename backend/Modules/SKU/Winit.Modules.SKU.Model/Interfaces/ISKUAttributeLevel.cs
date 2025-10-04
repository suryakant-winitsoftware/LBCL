using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUAttributeLevel
    {
       public List<ISelectionItem>? SKUGroupTypes { get; set; }

       public Dictionary<string, List<ISelectionItem>>? SKUGroups { get; set; }
    }
}
