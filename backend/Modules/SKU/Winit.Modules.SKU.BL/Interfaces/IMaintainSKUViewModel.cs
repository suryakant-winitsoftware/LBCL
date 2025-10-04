using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.CustomControls;

namespace Winit.Modules.SKU.BL.Interfaces
{
    /// <summary>
    /// Interface for maintaining SKU functionality
    /// </summary>
    public interface IMaintainSKUViewModel
    {
        /// <summary>
        /// Gets or sets the list of SKU items for grid display
        /// </summary>
        List<ISKUListView> MaintainSKUGridList { get; set; }

        /// <summary>
        /// Gets or sets the current page number
        /// </summary>
        int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total count of SKU items
        /// </summary>
        int TotalSKUItemsCount { get; set; }

        /// <summary>
        /// Gets or sets the list of attribute type selection items
        /// </summary>
        List<ISelectionItem> AttributeTypeSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the SKU attribute level information
        /// </summary>
        ISKUAttributeLevel SkuAttributeLevel { get; set; }

        /// <summary>
        /// Gets or sets the list of attribute name selection items
        /// </summary>
        List<ISelectionItem> AttributeNameSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the list of product division selection items
        /// </summary>
        List<ISelectionItem> ProductDivisionSelectionItems { get; set; }

        /// <summary>
        /// Gets or sets the list of status selection items
        /// </summary>
        List<ISelectionItem> StatusSelectionItems { get; set; }

        /// <summary>
        /// Initializes the status selection items
        /// </summary>
        Task GetStatus();

        /// <summary>
        /// Applies filter criteria to the SKU list
        /// </summary>
        /// <param name="filterCriterias">List of filter criteria to apply</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ApplyFilter(List<FilterCriteria> filterCriterias);

        /// <summary>
        /// Resets all applied filters
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ResetFilter();

        /// <summary>
        /// Applies sorting criteria to the SKU list
        /// </summary>
        /// <param name="sortCriteria">Sort criteria to apply</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ApplySort(SortCriteria sortCriteria);

        /// <summary>
        /// Populates the view model with data
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PopulateViewModel();

        /// <summary>
        /// Deletes a SKU item by its UID
        /// </summary>
        /// <param name="uid">The UID of the SKU to delete</param>
        /// <returns>Task containing the result message</returns>
        Task<string> DeleteSKUItem(string? uid);

        /// <summary>
        /// Gets the attribute type information
        /// </summary>
        /// <returns>Task containing the SKU attribute level information</returns>
        Task<ISKUAttributeLevel> GetAttributeType();

        /// <summary>
        /// Handles page index changes
        /// </summary>
        /// <param name="pageNumber">The new page number</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PageIndexChanged(int pageNumber);

        /// <summary>
        /// Handles attribute type selection
        /// </summary>
        /// <param name="code">The selected attribute type code</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task OnAttributeTypeSelect(string code);

        /// <summary>
        /// Handles division selection type changes
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task OnDivisionSelectionTypeSelect();

        /// <summary>
        /// Applies filter criteria to the SKU list
        /// </summary>
        /// <param name="columnsForFilter">List of filter models</param>
        /// <param name="filterCriteria">Dictionary of filter criteria</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task OnFilterApply(List<UIModels.Common.Filter.FilterModel> columnsForFilter, Dictionary<string, string> filterCriteria);

        /// <summary>
        /// Checks if a user exists by code
        /// </summary>
        /// <param name="code">The code to check</param>
        /// <returns>Task containing whether the user exists</returns>
        Task<bool> CheckUserExistsAsync(string code);
    }
}
