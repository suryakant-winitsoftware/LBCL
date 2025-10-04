using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Interfaces
{
    public interface IAuthMaster
    {
        IEmp Emp { get; set; }
        Winit.Modules.JobPosition.Model.Interfaces.IJobPosition JobPosition { get; set; }
        Winit.Modules.Role.Model.Interfaces.IRole Role { get; set; }
        List<Winit.Modules.Vehicle.Model.Interfaces.IVehicleStatus>? VehicleStatuses { get; set; }
    }
}
