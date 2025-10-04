using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.StoreMaster.Model.Interfaces;

namespace Winit.Modules.StoreMaster.Model.Classes
{
    public class StoreViewModelDCO 
    {
       public Winit.Modules.Store.Model.Classes.Store store { get; set; }
       public Winit.Modules.Store.Model.Classes.StoreAdditionalInfo StoreAdditionalInfo { get; set; }
       public List<Winit.Modules.Store.Model.Classes.StoreCredit> StoreCredits { get; set; }
       public List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument> StoreDocuments { get; set; }
       public List<Winit.Modules.Address.Model.Classes.Address> Addresses { get; set; }
       public List<Winit.Modules.Contact.Model.Classes.Contact> Contacts { get; set; }
      

    }

    public class StoreViewModelDCO1: IStoreViewModelDCO
    {
        public Winit.Modules.Store.Model.Interfaces.IStore store { get; set; }
        public Winit.Modules.Store.Model.Interfaces.IStoreAdditionalInfo StoreAdditionalInfo { get; set; }
        public List<Winit.Modules.Store.Model.Interfaces.IStoreCredit> StoreCredits { get; set; }
        public List<Winit.Modules.StoreDocument.Model.Interfaces.IStoreDocument> StoreDocuments { get; set; }
        public List<Winit.Modules.Address.Model.Interfaces.IAddress> addresses { get; set; }
        public List<Winit.Modules.Contact.Model.Interfaces.IContact> Contacts { get; set; }


    }

}
