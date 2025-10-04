using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.FileSys.Model.Classes
{
    public class SKUImage : Winit.Modules.FileSys.Model.Interfaces.ISKUImage
    {
        public string SKUUID { get; set; }
        public string FileSysUID { get; set; }
        public bool IsDefault { get; set; }
    }
}
