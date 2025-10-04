using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Classes;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Mapping.BL.Interfaces
{
    public interface IMappingViewModel
    {
        public MappingComponentDTO MappingDTO { get; }

        //public List<ISelectionItem>? LocationTypesSelectionItems { get; set; }
        //public List<ISelectionItem>? LocationsSelectionItems { get; set; }
        //public List<ISelectionItem>? StoreGroupTypesSelectionItems { get; set; }
        //public List<ISelectionItem>? StoreGroupsSelectionItems { get; set; }
        //public List<DataGridColumn>? dataGridColumns { get; set; }
        //public List<IMappingItemView>? GridDataSource { get; set; }
        //public List<ISelectionItem> TabSelectionItems { get; set; }
        //public int TabIndex { get; set; }
        //public bool IsExcluded { get; set; }
        //public string? LinkedItemUID { get; set; }
        //public string? LinkedItemType { get; set; }
        Task PopulateViewModel(string? linkedItemUID = null, string? linkedItemType = null);
        Task LocationTypeDDClick(ISelectionItem selectedLocationType);
        Task StoreGroupTypeDDClick(ISelectionItem selectedStoreGroupType);
        void OnTabClick(ISelectionItem selectionItem);
        List<string> AddLocationToGrid();
        List<string> AddStoreToGrid();
        Task<bool> SaveMapping();
    }
}
