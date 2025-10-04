using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Mapping.Model.Classes
{
    public class MappingComponentDTO
    {
        public string LinkedItemUID { get; set; }
        public string LinkedItemType { get; set; }
        public bool IsExcluded { get; set; }
        public int TabIndex { get; set; } = 0;
        public List<ISelectionItem> LocationTypesSelectionItems { get; } = [];
        public List<ISelectionItem> LocationsSelectionItems { get; } = [];
        public List<ISelectionItem> StoreGroupTypesSelectionItems { get; } = [];
        public List<ISelectionItem> StoreGroupsSelectionItems { get; } = [];
        public List<ISelectionItem> TabSelectionItems { get; } = [];

        public List<ISelectionItem> DistributorTypeSelectionItems { get; } = [];
        public List<ISelectionItem> DistributorSelectionItems { get; } = [];
        public List<ISelectionItem> UserTypeSelectionItems { get; } = [];
        public List<ISelectionItem> UserSelectionItems { get; } = [];

        public List<IMappingItemView>? GridDataSource { get; } = [];
        public List<string> DuplicateValues { get; } = new List<string>();
        public List<DataGridColumn> DataGridColumns { get; } = new List<DataGridColumn>();

        /// <summary>
        /// Action methodes.
        /// </summary>

        public Func<List<string>> OnLocationClick { get; set; }
        public Func<List<string>> OnStoreGroupClick { get; set; }
        public Func<List<string>> OnDistributorClick { get; set; }
        public Func<List<string>> OnUserClick { get; set; }
        public Action<ISelectionItem> OnTabClick { get; set; }
        public Action<ISelectionItem>? OnLocationTypeDDClick { get; set; }
        public Action<ISelectionItem>? OnStoreGroupTypeDDClick { get; set; }
    }
}
