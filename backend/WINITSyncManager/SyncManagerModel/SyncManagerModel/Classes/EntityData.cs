using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class EntityData : SyncBaseModel, IEntityData
    {
        public string EntityName { get; set; }
        public string EntityGroup { get; set; }
        public string SelectQuery { get; set; }
        public string InsertQuery { get; set; }
        public string MaxCount { get; set; }
    }
}
