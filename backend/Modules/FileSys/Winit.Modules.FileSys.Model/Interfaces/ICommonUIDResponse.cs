using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Classes;

namespace Winit.Modules.FileSys.Model.Interfaces
{
    public interface ICommonUIDResponse 
    {
        public string UID { get; set; }
        public int Id { get; set; }
    }
}
