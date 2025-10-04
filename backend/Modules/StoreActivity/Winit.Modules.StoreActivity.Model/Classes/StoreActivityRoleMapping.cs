using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.Model.Classes
{
    public class StoreActivityRoleMapping : BaseModel, IStoreActivityRoleMapping
    {
        public string StoreActivityUID { get; set; }
        public string RoleUID { get; set; }
    }
}
