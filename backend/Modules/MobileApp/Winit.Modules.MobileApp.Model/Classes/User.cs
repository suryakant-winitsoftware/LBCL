using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mobile.Model.Interfaces;

namespace Winit.Modules.Mobile.Model.Classes
{
    public class User : IUser
    {
        public string UID { get; set; }
        public string Name { get; set; }
    }
}
