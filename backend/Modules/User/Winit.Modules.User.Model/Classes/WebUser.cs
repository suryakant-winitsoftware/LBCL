using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.User.Model.Classes
{
    public class WebUser
    {
        public Emp.Model.Classes.Emp Emp { get; set; }
        public JobPosition.Model.Classes.JobPosition JobPosition { get; set; }
        public Winit.Modules.Role.Model.Classes.Role Role { get; set; }
    }
}
