using SyncManagerModel.Base;

namespace SyncManagerModel.Interfaces
{
    public interface IPrepareDB : ISyncBaseModel
    {         
        public string Script { get; set; }
        public string EnityName { get; set; }
        public string EnityGroup { get; set; }
        public string TablePrefix { get; set; }

    }
}
