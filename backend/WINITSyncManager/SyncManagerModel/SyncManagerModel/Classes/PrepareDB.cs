using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class PrepareDB :SyncBaseModel, IPrepareDB
    {
        public string Script { get; set; }
        public string EnityName { get; set; }
        public string EnityGroup { get; set; }
        public string TablePrefix { get; set; }
    }
}
