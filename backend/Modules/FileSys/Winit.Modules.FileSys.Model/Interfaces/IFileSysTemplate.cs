using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.FileSys.Model.Interfaces
{
    public interface IFileSysTemplate:IBaseModel    
    {
        public string FileSysType { get; set; } 
        public string Folder { get; set; } 
        public string RelativePathFormat { get; set; } 
        public bool IsMobile { get; set; } 
        public bool IsServer { get; set; } 
    }
}
