using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.AuditTrail.Model.Classes
{
    public class AuditTrailEntry : IAuditTrailEntry
    {
        public string Id { get; set; } // _id field in MongoDB
        public DateTime? ServerCommandDate { get; set; } // Date and time of the command
        public string LinkedItemType { get; set; } // E.g., "SalesOrder"
        public string LinkedItemUID { get; set; }  // Unique ID for the item, e.g., "SO001"
        public string CommandType { get; set; }       // Action type, e.g., "Insert", "Update", "Delete"
        public DateTime CommandDate { get; set; } // Date and time of the command
        public string DocNo { get; set; }         // Document number
        public string? JobPositionUID { get; set; }           
        public string EmpUID { get; set; }           
        public string EmpName { get; set; }           
        public Dictionary<string, object> NewData { get; set; } = new Dictionary<string, object>();       // Original new data (for reference)
        public string? OriginalDataId { get; set; }
        public bool HasChanges { get; set; }
        public List<ChangeLog>? ChangeData { get; set; } // List of changes
    }

}
