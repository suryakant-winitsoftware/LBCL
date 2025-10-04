using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class OnBoardCustomerDTO:IOnBoardCustomerDTO
    {
        public IStore? Store { get; set; }
        public IStoreAdditionalInfo? StoreAdditionalInfo { get; set; }
        public IStoreAdditionalInfoCMI? StoreAdditionalInfoCMI { get; set; }
        public List<IStoreCredit>? StoreCredit { get; set; }
        public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>? FileSys { get; set; }
    }
}
