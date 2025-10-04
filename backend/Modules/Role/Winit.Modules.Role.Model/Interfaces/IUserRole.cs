using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Role.Model.Interfaces
{
    public interface IUserRole:IBaseModel
    {
        string Name { get; set; }
        bool IsAdmin { get; set; }
        bool IsDistributorUser { get; set; }
        bool IsPrincipleUser { get; set; }
        bool IsAppUser { get; set; }
        bool IsCPEUser { get; set; }
        string AliasDisignation { get; set; }
    }
}
