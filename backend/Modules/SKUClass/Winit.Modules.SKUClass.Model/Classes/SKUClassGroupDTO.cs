using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIClasses;
using Winit.Modules.SKUClass.Model.UIInterfaces;

namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClassGroupDTO
    {
        public SKUClassGroup? SKUClassGroup { get; set; }
        public List<SKUClassGroupItemView>? SKUClassGroupItems { get; set; }
    }
}
