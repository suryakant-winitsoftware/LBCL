using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.User.Model.Interfaces;

namespace Winit.Modules.User.Model.Classes
{
    public class MaintainUser :  IMaintainUser 
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string EmpNo { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LoginId { get; set; }
        public string AuthType { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? ApprovalStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool CanHandleStock { get; set; }

    }
}
