using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Enums
{
    public enum ResponseStatus
    {
        Success = 200,
        Failure = -1,
        AnotherDeviceLogin = 201
    }
}
