using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.AwayPeriod.Model.Interfaces
{
    public interface IAwayPeriod : IBaseModel
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
