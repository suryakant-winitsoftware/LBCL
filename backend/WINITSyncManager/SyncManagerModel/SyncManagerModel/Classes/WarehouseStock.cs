using SyncManagerModel.Base;
using SyncManagerModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Classes
{
    public class WarehouseStock : SyncBaseModel, IWarehouseStock
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
