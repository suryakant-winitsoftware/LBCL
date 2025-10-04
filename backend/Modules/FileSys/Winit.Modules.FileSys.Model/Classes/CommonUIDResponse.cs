using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.Modules.FileSys.Model.Classes
{
    public class CommonUIDResponse : ICommonUIDResponse
    {
        public string UID { get; set; }
        public int Id { get; set; }
    }
}
