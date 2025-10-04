using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IApprovalLevel
    {
        public string Level { get; set; }
        public string Status { get; set; }
        public bool IsExtend { get; set; }
    }
}
