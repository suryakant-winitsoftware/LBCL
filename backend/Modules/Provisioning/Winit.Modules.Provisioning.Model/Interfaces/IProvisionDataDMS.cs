using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Provisioning.Model.Interfaces
{
    public interface IProvisionDataDMS : IBaseModel
    {
        public string UID { get; set; }
       
    }
}
