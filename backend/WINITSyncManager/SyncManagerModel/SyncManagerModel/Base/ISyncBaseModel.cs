namespace SyncManagerModel.Base
{
    public interface ISyncBaseModel
    {
        public string? Source { get; set; }
        public int? IsProcessed { get; set; }
        public string? InsertedOn { get; set; }
        public string? ProcessedOn { get; set; }
        public string? ErrorDescription { get; set; }
        public string? CommonAttribute1 { get; set; }
        public string? CommonAttribute2 { get; set; }
    }
}
