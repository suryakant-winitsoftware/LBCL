using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.FileSys.Model.Interfaces
{
    public interface ISKUImage 
    {
        public string SKUUID { get; set;}
        public string FileSysUID { get; set;}
        public bool IsDefault { get; set;}
        
    }
}
