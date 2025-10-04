using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.AuditTrail.Model.Classes
{
    public interface IChangeLog
    {
        string Field { get; set; }
        object OldValue { get; set; }
        object NewValue { get; set; }
    }

}
