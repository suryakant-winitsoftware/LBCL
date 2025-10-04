using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;

namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClassGroupMaster : ISKUClassGroupMaster
    {
        public ISKUClassGroup? SKUClassGroup { get; set; }
        public List<ISKUClassGroupItemView>? SKUClassGroupItems { get; set; }
    }
}
