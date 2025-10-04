using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.Model.Interface;

namespace Winit.Modules.User.Model.Classes
{
    public class UserLocationMapping : IUserLocationMapping
    {
        public string MappingCode { get; set; }
        public string MappingName { get; set; }
        public bool IsActive { get; set; }
    }
}
