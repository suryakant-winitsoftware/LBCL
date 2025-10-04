using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes
{
    public class UserLogin : IUserLogin
    {
        public string UserID { get; set; }
        public string Password { get; set; }
        public string ChallengeCode { get; set; }
        public string DeviceId { get; set; }
    }
}
