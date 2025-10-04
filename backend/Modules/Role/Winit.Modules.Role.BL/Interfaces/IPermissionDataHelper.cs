using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Role.Model.Interfaces;

namespace Winit.Modules.Role.BL.Interfaces
{
    public interface IPermissionDataHelper
    {
        Task<IPermission> GetPermissionByPage(string roleUID, bool isPrincipLErOLE, string pageRoute    );
    }
}
