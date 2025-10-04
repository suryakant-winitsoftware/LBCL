using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.AwayPeriod.Model.Interfaces;

namespace Winit.Modules.AwayPeriod.Model.Classes
{
    public class AwayPeriod : BaseModel, IAwayPeriod
    {
        public string OrgUID { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string Description { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsActive { get; set; }
    }
}
