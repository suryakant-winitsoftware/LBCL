using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;

namespace Winit.Modules.Common.BL.Classes
{
    public class ScopedService: IScopedService, IDisposable
    {
        public Guid ScopeIdentifier { get; set; }

        public ScopedService()
        {
            ScopeIdentifier = Guid.NewGuid();
        }

        public void Dispose()
        {

        }
    }
}
