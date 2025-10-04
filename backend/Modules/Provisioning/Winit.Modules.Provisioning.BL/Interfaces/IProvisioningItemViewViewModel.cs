using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Provisioning.Model.Interfaces;

namespace Winit.Modules.Provisioning.BL.Interfaces
{
    public interface IProvisioningItemViewViewModel
    {
        public List <IProvisionItemView> ProvisionItemDataList { get; set; }
        public IProvisionItemView ProvisionItemViewDetails { get; set; }
        Task GetProvisioningItemDetailsByUID(string provisionItemUID);
        Task GetProvisionItemViewData(string? UID);
    }
}
