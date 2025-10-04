using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Auth.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Auth.Model.Classes
{
    public class LoginResponse: ILoginResponse
    {
        public int StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
        public ApiResponse<string> ApiResponse { get; set; }
        public string Token { get; set; } = string.Empty;
        public IAuthMaster AuthMaster { get; set; }
    }
}
