using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Address.Model.Interfaces;
using Winit.Modules.Contact.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class OnBoardEditCustomerDTO : IOnBoardEditCustomerDTO
    {
        public Store? Store { get; set; }
        public StoreAdditionalInfo? StoreAdditionalInfo { get; set; }
        public StoreAdditionalInfoCMI? StoreAdditionalInfoCMI { get; set; }
        public Winit.Modules.Contact.Model.Classes.Contact? Contact { get; set; }
        public List<Winit.Modules.Address.Model.Classes.Address> Address { get; set; }
        public List<Winit.Modules.FileSys.Model.Classes.FileSys>? FileSys { get; set; }
        public List<Winit.Modules.Store.Model.Classes.StoreCredit>? StoreCredit { get; set; }
        public List<Winit.Modules.Store.Model.Classes.AsmDivisionMapping>? AsmDivisionMapping { get; set; }
    }
}
