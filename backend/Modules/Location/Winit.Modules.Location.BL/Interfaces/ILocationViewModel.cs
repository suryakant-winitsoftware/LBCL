using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Location.BL.Interfaces
{
    public interface ILocationViewModel
    {
        public List<ILocationItemView> LocationItemViews { get; set; }
        public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
        Task PopulateViewModel();
        Task GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task<List<ISelectionItem>> GetLocationTypeSelectionItems(int Level, bool IsAddItembtn, string ParentUID = null, bool IsAll = false);
        Task<bool> CreateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task CreateLocationHierarchy(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task<bool> DeleteLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task<bool> UpdateLocation(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task<ILocationItemView> CreateClone(Winit.Modules.Location.Model.Interfaces.ILocationItemView locationItemView);
        Task LocationTypeSelectedForParent(Winit.Modules.Location.Model.Interfaces.ILocationItemView Context, String UID);
        Task ApplyFilter(IDictionary<string, string> keyValuePairs);
    }
}
