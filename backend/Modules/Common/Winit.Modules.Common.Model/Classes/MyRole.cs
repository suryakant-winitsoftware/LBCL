using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Common.Model.Interfaces;

namespace Winit.Modules.Common.Model.Classes
{
    public class MyRole:IMyRole
    {
        public string JobPositionUID { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
    }
}
