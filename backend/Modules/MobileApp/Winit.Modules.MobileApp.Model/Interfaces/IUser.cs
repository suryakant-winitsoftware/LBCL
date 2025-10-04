using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Mobile.Model.Interfaces
{
    public interface IUser 
    {
        public string UID { get; set; }
        public string Name { get; set; }
        
    }
}
