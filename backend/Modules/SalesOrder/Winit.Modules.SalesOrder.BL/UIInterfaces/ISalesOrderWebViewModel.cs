using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.SalesOrder.BL.UIInterfaces
{
    public interface ISalesOrderWebViewModel : ISalesOrderViewModel
    {
        bool PresalesDeliveryDistributorEnabled { get; }
        Task OnAddProductClick(List<ISKUV1> skus);
        List<IStoreCredit> StoreCredits { get; set; }
        List<ISelectionItem> StoreCreditsSelectionItems { get; set; }
        List<ISelectionItem> DeliveryDistributor { get; set; }
        void OnStoreDistributionChannelSelect(string distributionChannelUID);
        void OnDeliveryDistributorSelect(DropDownEvent dropDownEvent);
    }
}
