using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.AuditTrail.Model.Classes
{
    public class AuditDataEntry
    {
        public string DataType { get; set; } // "Old" or "New"
        public Dictionary<string, object> Data { get; set; } // Key-value pairs of field names and their values
    }
}
