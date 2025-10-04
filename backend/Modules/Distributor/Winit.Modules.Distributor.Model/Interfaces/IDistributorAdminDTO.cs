using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.JobPosition.Model.Interfaces;

namespace Winit.Modules.Distributor.Model.Interfaces
{
    public interface IDistributorAdminDTO
    {
        IEmp Emp { get; set; }
        IJobPosition JobPosition { get; set; }
        DistributorAdminActionType ActionType { get; set; }
    }
    public enum DistributorAdminActionType
    {
        Add,UpdateUserName, UpdatePW, Delete
    }
}
