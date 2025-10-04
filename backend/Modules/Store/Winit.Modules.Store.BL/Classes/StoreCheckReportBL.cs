using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreCheckReportBL: IStoreCheckReportBL
    {
        protected readonly Winit.Modules.Store.DL.Interfaces.IStoreCheckReportDL _storeCheckReportDL;
        IServiceProvider _serviceProvider;
       
        public StoreCheckReportBL(DL.Interfaces.IStoreCheckReportDL storeCheckReportDL,  IServiceProvider serviceProvider)
        {
            _storeCheckReportDL = storeCheckReportDL;
            _serviceProvider = serviceProvider;
            
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IStoreCheckReport>> GetStoreCheckReportDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeCheckReportDL.GetStoreCheckReportDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<List<Winit.Modules.Store.Model.Interfaces.IStoreCheckReportItem>> GetStoreCheckReportItems(string uid)
        {
            return await _storeCheckReportDL.GetStoreCheckReportItems(uid);
        }
    }
}
