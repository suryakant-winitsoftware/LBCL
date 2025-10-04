using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class ApprovalLevel : IApprovalLevel
    {
        public string Level { get; set; }
        public string Status { get; set; }
        public bool IsExtend { get; set; }
    }
}
