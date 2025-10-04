using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Syncing.Model.Interfaces;

namespace Winit.Modules.Syncing.Model.Classes
{
    public class TableGroup :BaseModel, ITableGroup
    {
        public string GroupName { get; set; }
        public bool IsActive { get; set; }
        public bool SerialNo { get; set; }
        public DateTime? LastUploadTime { get; set; }
        public DateTime? LastDownloadTime { get; set; }
    }
}
