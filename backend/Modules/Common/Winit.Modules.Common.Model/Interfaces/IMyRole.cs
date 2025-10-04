using System;
using System.Collections.Generic;
using System.Text;

namespace Winit.Modules.Common.Model.Interfaces
{
    public interface IMyRole
    {
        public string JobPositionUID { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
    }
}
