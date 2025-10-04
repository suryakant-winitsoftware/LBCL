using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreMaster 
    {
        public Winit.Modules.Store.Model.Interfaces.IStore Store { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo StoreAdditionalInfo { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> storeCredits { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreAttributes> storeAttributes { get; set; }
        public List<Winit.Modules.Address.Model.Interfaces.IAddress> Addresses { get; set; }
        public List<Winit.Modules.Contact.Model.Interfaces.IContact> Contacts { get; set; }
        public List<StandardListSource> OrgList { get; set; }
        public List<StandardListSource> DistributionChannelList { get; set; }
    }
}
