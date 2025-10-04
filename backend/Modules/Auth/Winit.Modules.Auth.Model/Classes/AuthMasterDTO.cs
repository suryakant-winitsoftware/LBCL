using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;

namespace Winit.Modules.Auth.Model.Classes;

public class AuthMasterDTO
{
    public Winit.Modules.Emp.Model.Classes.Emp Emp { get; set; }
    public Winit.Modules.JobPosition.Model.Classes.JobPosition JobPosition { get; set; }
    public List<Winit.Modules.Vehicle.Model.Classes.VehicleStatus> VehicleStatuses { get; set; }
}
