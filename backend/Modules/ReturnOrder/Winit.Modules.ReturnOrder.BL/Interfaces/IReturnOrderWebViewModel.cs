using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.ReturnOrder.BL.Interfaces;

public interface IReturnOrderWebViewModel : IReturnOrderViewModel
{
    public List<IStoreCredit> StoreCredits { get; set; }
    public List<ISelectionItem> StoreCreditsSelectionItems { get; set; }
    public void OnStoreDistributionChannelSelect(string distributionChannelUID);
}
