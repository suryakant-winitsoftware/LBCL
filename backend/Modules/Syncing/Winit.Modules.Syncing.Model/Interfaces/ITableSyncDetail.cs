using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Syncing.Model.Interfaces
{
    public interface ITableSyncDetail
    {
        public string TableName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public int RecordCount { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}
