using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.BL.Interfaces;

public interface ILocationTypeViewModel
{
    public List<ILocationTypeItemView> LocationTypeItemViews { get; set; }
    public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
    Task PopulateViewModel();
    Task GetChildGrid(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView);
    Task<bool> CreateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView);
    Task<bool> DeleteLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView);
    Task<bool> UpdateLocationType(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView);
    Task<ILocationTypeItemView> CreateClone(Winit.Modules.Location.Model.Interfaces.ILocationTypeItemView locationTypeItemView);
    Task ApplyFilter(IDictionary<string, string> keyValuePairs);
}
