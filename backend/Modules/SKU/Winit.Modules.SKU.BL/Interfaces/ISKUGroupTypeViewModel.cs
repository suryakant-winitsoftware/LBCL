using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKU.BL.Interfaces
{
    public interface ISKUGroupTypeViewModel
    {
        public List<ISKUGroupTypeItemView> SKUGroupTypeItemViews { get; set; }
        public List<Shared.Models.Enums.FilterCriteria> FilterCriterias { get; set; }
        Task PopulateViewModel();
        Task GetChildGrid(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView);
        Task<bool> CreateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView);
        Task<bool> DeleteSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView);
        Task<bool> UpdateSKUGroupType(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView);
        Task<ISKUGroupTypeItemView> CreateClone(Winit.Modules.SKU.Model.Interfaces.ISKUGroupTypeItemView sKUGroupTypeItemView);
        Task ApplyFilter(IDictionary<string, string> keyValuePairs);
    }
}
