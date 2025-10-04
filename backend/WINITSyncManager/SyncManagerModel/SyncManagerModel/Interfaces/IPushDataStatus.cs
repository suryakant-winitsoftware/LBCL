using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IPushDataStatus :ISyncBaseModel
    {
        public string? UID { get; set; } 
        public string? LinkedItemUid { get; set; } 
        public string? LinkedItemType { get; set; } 
        public string? Status { get; set; } 
        public string? ErrorMessage { get; set; } 
        public DateTime ModifiedTime { get; set; } 

    }
}
