using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{
    public class ApprovalActionResponse
    {
        public bool IsSuccess {get; set;}
        public bool IsApprovalActionRequired { get; set; }

    }
}
