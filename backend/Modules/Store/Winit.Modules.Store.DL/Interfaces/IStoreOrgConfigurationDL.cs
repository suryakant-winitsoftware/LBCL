using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.DL.Interfaces
{
    public interface IStoreOrgConfigurationDL
    {
        Task<int> CreateStoreOrgConfiguratoion(OrgConfigurationUIModel orgConfiguration);
        Task<Model.Interfaces.IOrgConfiguration> SelectStoreOrgConfigurationByStoreUID(string storeUID);
    }
}
