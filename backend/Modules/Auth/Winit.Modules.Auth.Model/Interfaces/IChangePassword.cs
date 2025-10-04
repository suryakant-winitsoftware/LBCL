using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Auth.Model.Interfaces
{
    public interface IChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; } 
        public string ChallengeCode { get; set; } 
        public string UserId { get; set; }
        public string EmpUID { get; set; }

    }
}
