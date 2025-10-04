using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface IEntityData : ISyncBaseModel
    {
        public string EntityName { get; set; }
        public string EntityGroup { get; set; }
        public string SelectQuery  { get; set; }
        public string InsertQuery { get; set; }
        public string MaxCount { get; set; }

    }
}
