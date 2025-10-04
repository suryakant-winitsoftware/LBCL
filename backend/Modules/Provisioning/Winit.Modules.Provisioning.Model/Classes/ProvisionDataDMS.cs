using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Provisioning.Model.Interfaces;

namespace Winit.Modules.Provisioning.Model.Classes
{
    public class ProvisionDataDMS : BaseModel , IProvisionDataDMS
    {
        public string UID { get; set; }
        
    }
}
