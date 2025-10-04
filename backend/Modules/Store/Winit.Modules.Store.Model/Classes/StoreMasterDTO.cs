using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreMasterDTO
    {
        public Winit.Modules.Store.Model.Classes.Store Store { get; set; }
        public Winit.Modules.Store.Model.Classes.StoreAdditionalInfo StoreAdditionalInfo { get; set; }
        public List<Winit.Modules.Store.Model.Classes.StoreCredit> storeCredits { get; set; }
        public List<Winit.Modules.Store.Model.Classes.StoreAttributes> storeAttributes { get; set; }
        public List<Winit.Modules.Address.Model.Classes.Address> Addresses { get; set; }
        public List<Winit.Modules.Contact.Model.Classes.Contact> Contacts { get; set; }
        public List<StandardListSource> OrgList { get; set; }
        public List<StandardListSource> DistributionChannelList { get; set; }
    }
}
