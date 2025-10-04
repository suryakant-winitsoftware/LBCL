using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.Model.Interfaces;
using Winit.Modules.Scheme.BL.Classes;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;

namespace Winit.Modules.Scheme.BL.Interfaces
{
    public  interface ISellOutSchemeHeaderItemViewModel : ISchemeViewModelBase
    {
        ISellOutMasterScheme SellOutMaster { get; set; }
        
        public List<Winit.Shared.Models.Common.ISelectionItem> SellOutReasons { get; set; }
        
        public List<IPreviousOrders> PreviousOrdersList { get; set; }
        Task PopulateViewModel();
        Task SaveDataAsync(ISellOutSchemeHeaderItem item);
        Task<List<ISKUMaster>> GetSKUsMasterBySKUUIDs(SKUMasterRequest sKUMasterRequest);
        
        //Task<List<ISKU>> GetSKUsByOrgUID(string OrgUID);

       
        Task GetSkusByOrgUID();
        Task HandleSelectedCustomers(List<IPreviousOrders> selectedCustomers);
       
        Task<bool> DeleteSellOutLine();
        Task ApproveSellOutSchemeHeaderItemAsync(string uid);
        
    }
}
