using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.Model.Classes
{
    public class TableSyncDetail: ITableSyncDetail
    {
        public string TableName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int RecordCount { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}
