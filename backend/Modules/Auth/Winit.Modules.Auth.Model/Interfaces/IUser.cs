using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Auth.Model.Interfaces
{
    public interface IUser
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string CompanyUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LoginId { get; set; }
        public string EmpNo { get; set; }
    }
}
