using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreOrgConfigurationBL : Interfaces.IStoreOrgConfigurationBL
    {
        IStoreOrgConfigurationDL _storeOrgConfigurationDL;
        public StoreOrgConfigurationBL(IStoreOrgConfigurationDL storeOrgConfigurationDL)
        {
            _storeOrgConfigurationDL = storeOrgConfigurationDL;
        }

        public Task<int> CreateStoreOrgConfiguratoion(OrgConfigurationUIModel orgConfiguration)
        {
            return _storeOrgConfigurationDL.CreateStoreOrgConfiguratoion(orgConfiguration);
        }
        public Task<Model.Interfaces.IOrgConfiguration> SelectStoreOrgConfigurationByStoreUID(string storeUID)
        {
            return _storeOrgConfigurationDL.SelectStoreOrgConfigurationByStoreUID(storeUID);
        }

    }
}
