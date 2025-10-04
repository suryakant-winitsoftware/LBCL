using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.FileSys.Model.Interfaces;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreDTO
    {
        public IStore? Store { get; set; }
        public IStoreAdditionalInfo? StoreAdditionalInfo { get; set; }
        public IStoreAdditionalInfoCMI? StoreAdditionalInfoCMI { get; set; }
        public IFileSys? FileSys { get; set; }
    }
}
