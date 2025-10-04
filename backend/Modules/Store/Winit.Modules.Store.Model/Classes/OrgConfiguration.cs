using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class OrgConfiguration: IOrgConfiguration
    {
        public List<IStoreCredit> StoreCredit {  get; set; }
        public List<IStoreAttributes> StoreAttributes { get; set; }
    }
    public class OrgConfigurationUIModel
    {
        public List<StoreCredit> StoreCredit { get; set; }
        public List<StoreAttributes> StoreAttributes { get; set; }
    }
}
