using Winit.Modules.Auth.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes
{
    public class AuthMaster : IAuthMaster
    {
        public IEmp Emp { get; set; }
        public Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition { get; set; }
        public Winit.Modules.Role.Model.Interfaces.IRole Role { get; set; }
        public List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>? VehicleStatuses { get; set; }
    }
}
