using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Auth.Model.Interfaces
{
    public interface IUserLogin
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}
