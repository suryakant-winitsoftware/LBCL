using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Distributor.Model.Classes
{
    public class DistributorMasterView
    {
        public Org.Model.Classes.Org Org { get; set; }
        public Winit.Modules.Store.Model.Classes.Store Store { get; set; }
        public StoreAdditionalInfo StoreAdditionalInfo { get; set; }
        public StoreCredit StoreCredit { get; set; }
        public List<Contact.Model.Classes.Contact> Contacts { get; set; }
        public List<Winit.Modules.StoreDocument.Model.Classes.StoreDocument> Documents { get; set; }
        public Winit.Modules.Address.Model.Classes.Address Address {  get; set; }
        public List<Winit.Modules.Currency.Model.Classes.OrgCurrency> OrgCurrencyList {  get; set; }
    }
}
