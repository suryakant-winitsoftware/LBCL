using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;

namespace Winit.Modules.SKUClass.Model.UIInterfaces
{
    public interface ISKUClassGroupItemView:ISKUClassGroupItems
    {
        public string? SKUName { get; set; }
        public string? PlantName { get; set; }
    }
}
