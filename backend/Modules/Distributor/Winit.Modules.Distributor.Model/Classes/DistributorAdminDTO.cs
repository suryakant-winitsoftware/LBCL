using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Distributor.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;

namespace Winit.Modules.Distributor.Model.Classes
{
    public class DistributorAdminDTO : IDistributorAdminDTO
    {
        public IEmp Emp { get; set; }
        public IJobPosition JobPosition { get; set; }
        public DistributorAdminActionType ActionType { get; set; }
    }
}
