using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.BL.Interfaces
{
    public interface IStoreGroupTypeViewModel
    {
        public List<IStoreGroupTypeItemView> StoreGroupTypeItemViews { get; set; }
        public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
        Task PopulateViewModel();
        Task getChildGrid(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView);
        Task<bool> CreateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView);
        Task<bool> DeleteStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView);
        Task<bool> UpdateStoreGroupType(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView);
        Task<IStoreGroupTypeItemView> CreateClone(Winit.Modules.Store.Model.Interfaces.IStoreGroupTypeItemView storeGroupTypeItemView);
        Task ApplyFilter(IDictionary<string, string> keyValuePairs);
    }
}
