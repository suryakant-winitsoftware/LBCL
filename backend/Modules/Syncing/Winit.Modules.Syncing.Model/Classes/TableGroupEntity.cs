using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.Model.Classes
{
    public class TableGroupEntity :BaseModel, ITableGroupEntity
    {
        public string TableGroupUID { get; set; }
        public string TableName { get; set; }
        public bool HasUpload { get; set; }
        public bool HasDownload { get; set; }
        public DateTime? LastUploadedTime { get; set; }
        public DateTime? LastDownloadedTime { get; set; }
        public string MasterdataQuery { get; set; }
        public string SyncdataQuery { get; set; }
        public int SerialNo { get; set; }
        public bool IsActive { get; set; }
        public string SqliteInsertQuery { get; set; }
        public string SqliteInsertParameter { get; set; }
        public string ModelName { get; set; }
        public string SqliteUpdateQuery { get; set; }
    }
}
