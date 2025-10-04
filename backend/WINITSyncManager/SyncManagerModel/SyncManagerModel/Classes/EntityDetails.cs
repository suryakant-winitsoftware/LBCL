using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class EntityDetails : SyncBaseModel, IEntityDetails
    {
        public long SyncLogDetailId { get; set; }
        public string Entity { get; set; }
        public string UID { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public string TablePrefix { get; set; }
        public DateTime LastSyncTimeStamp { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Sequence { get; set; }
    }
}
