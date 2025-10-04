using Winit.Modules.Auth.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes
{
    public class User : IUser
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