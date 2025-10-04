using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IPermissionHeaderChecks
    {
        bool FullAccess { get; set; }
        bool Add { get; set; }
        bool Edit { get; set; }
        bool View { get; set; }
        bool Delete { get; set; }
        bool Download { get; set; }
        bool Approval { get; set; }
    }
}
