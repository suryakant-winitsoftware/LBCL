using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.StoreActivity.Model.Interfaces;

public interface IStoreActivityRoleMapping : IBaseModel
{
    public string StoreActivityUID { get; set; }
    public string RoleUID { get; set; }
}
