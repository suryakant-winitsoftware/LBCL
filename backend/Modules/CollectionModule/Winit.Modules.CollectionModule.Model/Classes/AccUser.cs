using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccUser : IAccUser
    {
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string Password { get; set; }
    }
}
