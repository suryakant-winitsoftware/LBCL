using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Classes;
using Winit.Modules.SKUClass.Model.UIInterfaces;

namespace Winit.Modules.SKUClass.Model.UIClasses
{
    public class SKUClassGroupItemView:SKUClassGroupItems, ISKUClassGroupItemView
    {
        public string? SKUName { get; set; }
        public string? PlantName { get; set; }
    }
}
