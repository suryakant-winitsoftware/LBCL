using SyncManagerModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IWarehouseStock : ISyncBaseModel
    {
        public long SyncLogId { get; set; }
        public string? UID { get; set; }
        public string? OrganisationUnit { get; set; }
        public string? WarehouseCode { get; set; }
        public string? SubWarehouseCode { get; set; }
        public string? SkuCode { get; set; }
        public int Qty { get; set; }
    }
}
