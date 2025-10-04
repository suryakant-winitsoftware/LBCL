using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.AuditTrail.Model.Classes
{
    public class AuditData
    {
        public string TableName { get; set; } // Name of the table, e.g., "SalesOrder", "SalesOrderLine"
        public string Action { get; set; }    // Action type, e.g., "Added", "Modified", "Deleted"
        public List<AuditDataEntry> Data { get; set; } = new List<AuditDataEntry>();
    }
}
