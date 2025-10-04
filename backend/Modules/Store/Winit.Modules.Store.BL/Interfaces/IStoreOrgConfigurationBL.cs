using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreOrgConfigurationBL
    {
        Task<int> CreateStoreOrgConfiguratoion(OrgConfigurationUIModel orgConfiguration);
        Task<Model.Interfaces.IOrgConfiguration> SelectStoreOrgConfigurationByStoreUID(string storeUID);
    }
}
