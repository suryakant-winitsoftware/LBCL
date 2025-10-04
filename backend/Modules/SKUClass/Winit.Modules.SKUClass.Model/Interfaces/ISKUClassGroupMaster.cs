using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKUClass.Model.UIInterfaces;

namespace Winit.Modules.SKUClass.Model.Interfaces
{
    public interface ISKUClassGroupMaster
    {
        public ISKUClassGroup? SKUClassGroup { get; set; }
        public List<ISKUClassGroupItemView>? SKUClassGroupItems { get; set; }
    }
}
