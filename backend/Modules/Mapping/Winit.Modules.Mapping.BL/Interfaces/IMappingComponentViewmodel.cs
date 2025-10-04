using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.LanguageResources.Web;

namespace Winit.Modules.Mapping.BL.Interfaces
{
    public interface IMappingComponentViewmodel
    {
        void DeleteItemFromGrid(IMappingItemView mappingItemView);
        List<DataGridColumn> GetDataGridColumns();
        Task InitializeAsync(string? linkedItemType, string? linkedItemUID);
        void OnLocationAddClick();
        Task OnLocationTypeDDClick(DropDownEvent dropDownEvent);
        void OnStoreGroupAddClick();
        Task OnStoreGroupTypeDDClick(DropDownEvent dropDownEvent);
        void OnTabClick(ISelectionItem selectionItem);
        Task<bool> SaveMapping();
    }
}
