using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Common.BL.Interfaces
{
    public interface IScopedService
    {
        public Guid ScopeIdentifier { get; }
    }
}
