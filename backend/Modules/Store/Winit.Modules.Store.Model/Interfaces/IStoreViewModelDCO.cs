using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Winit.Modules.StoreMaster.Model.Interfaces
{
    public interface IStoreViewModelDCO 
    {
        public Winit.Modules.Store.Model.Interfaces.IStore store { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo StoreAdditionalInfo { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> StoreCredits { get; set; }
        public List<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> StoreDocuments { get; set; }
        public List<Winit.Modules.Address.Model.Interfaces.IAddress> addresses { get; set; }
        public List<Winit.Modules.Contact.Model.Interfaces.IContact> Contacts { get; set; }
    }
}
