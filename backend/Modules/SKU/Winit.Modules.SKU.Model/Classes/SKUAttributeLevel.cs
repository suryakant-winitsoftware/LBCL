using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.Model.Classes
{
    public class SKUAttributeLevel : ISKUAttributeLevel
    {
        public List<ISelectionItem>? SKUGroupTypes { get; set; }

        public Dictionary<string, List<ISelectionItem>>? SKUGroups { get; set; }


    }
    public class SKUAttributeLevelDTO 
    {
        public List<SelectionItem>? SKUGroupTypes { get; set; }

        public Dictionary<string, List<SelectionItem>>? SKUGroups { get; set; }


    }
}
