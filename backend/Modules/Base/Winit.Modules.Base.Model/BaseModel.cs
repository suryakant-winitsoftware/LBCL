using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.Model.Classes.AuditTrail;

namespace Winit.Modules.Base.Model
{
    public class BaseModel:  Base.Model.IBaseModel
    {
        public int Id { get; set; } = 0;
        [AuditTrail]
        public string UID { get; set; }
        public int? SS { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? ServerAddTime { get; set; }
        public DateTime? ServerModifiedTime { get; set; }
        public string KeyUID { get; set; }
        public bool IsSelected { get; set; }


        // public string ErrorMessage { get; set; }
    }
}
