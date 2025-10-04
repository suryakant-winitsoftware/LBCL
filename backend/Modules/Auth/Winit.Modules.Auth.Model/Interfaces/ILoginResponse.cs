using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Interfaces
{
    public interface ILoginResponse
    {
        int StatusCode { get; set; }
        string? ErrorMessage { get; set; }
        public string Token { get; set; }
        public IAuthMaster AuthMaster { get; set; }
    }
}