namespace Winit.Modules.AuditTrail.Model.Classes
{
    public interface IAuditTrailEntry
    {
        string Id { get; set; } // _id field in MongoDB
        DateTime? ServerCommandDate { get; set; } // Date and time of the command
        string LinkedItemType { get; set; } // E.g., "SalesOrder"
        string LinkedItemUID { get; set; }  // Unique ID for the item, e.g., "SO001"
        string CommandType { get; set; }       // Action type, e.g., "Insert", "Update", "Delete"
        DateTime CommandDate { get; set; } // Date and time of the command
        string DocNo { get; set; }         // Document number
        string? JobPositionUID { get; set; }
        string EmpUID { get; set; }
        string EmpName { get; set; }
        Dictionary<string, object> NewData { get; set; }       // Original new data (for reference)
        string? OriginalDataId { get; set; }
        public bool HasChanges { get; set; }
        List<ChangeLog>? ChangeData { get; set; } // List of changes
    }

}
