using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class IsProcessedStatusUids : IIsProcessedStatusUids
    {
        public string? OracleUID { get; set; }
        public string? ErrorDescription { get; set; }
        public string? IsProcessed { get; set; }
        public string? ProcessedOn { get; set; }
        public string? CommonAttribute1 { get; set; }
        public string? CommonAttribute2 { get; set; }
    }
}
