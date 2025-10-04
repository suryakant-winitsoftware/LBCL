using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.BL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.SnackBar;


using Microsoft.AspNetCore.Components;
using Winit.Modules.Mapping.BL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIComponents.Common.LanguageResources.Web;



/// <summary>
/// Helper class for managing mapping component functionality
/// </summary>
/// 

namespace Winit.Modules.Mapping.BL.Classes;

public class MappingComponentViewmodel: IMappingComponentViewmodel
{
    private readonly IMappingViewModel _mappingViewModel;
    private readonly IToast _toast;
    private readonly IStringLocalizer<LanguageKeys> _localizer;

    public MappingComponentViewmodel(
        IMappingViewModel mappingViewModel,
        IToast toast,
        IStringLocalizer<LanguageKeys> localizer)
    {
        _mappingViewModel = mappingViewModel;
        _toast = toast;
        _localizer = localizer;
    }

    /// <summary>
    /// Initializes the mapping component with required data
    /// </summary>
    public async Task InitializeAsync(string? linkedItemType, string? linkedItemUID)
    {
        _mappingViewModel.MappingDTO.LinkedItemType = linkedItemType;
        _mappingViewModel.MappingDTO.LinkedItemUID = linkedItemUID;
        await _mappingViewModel.PopulateViewModel();
    }

    /// <summary>
    /// Handles tab selection in the mapping component
    /// </summary>
    public void OnTabClick(ISelectionItem selectionItem)
    {
        _mappingViewModel.OnTabClick(selectionItem);
    }

    /// <summary>
    /// Handles adding a location to the grid
    /// </summary>
    public void OnLocationAddClick()
    {
        List<string> duplicates = _mappingViewModel.AddLocationToGrid();
        if (duplicates.Any())
        {
            duplicates.ForEach(d => _toast.Add(_localizer["mapping"],
                d + _localizer["is_already_added"],
                Winit.UIComponents.SnackBar.Enum.Severity.Error));
        }
    }

    /// <summary>
    /// Handles adding a store group to the grid
    /// </summary>
    public void OnStoreGroupAddClick()
    {
        List<string> duplicates = _mappingViewModel.AddStoreToGrid();
        if (duplicates.Any())
        {
            duplicates.ForEach(d => _toast.Add(_localizer["mapping"],
                d + _localizer["is_already_added"],
                Winit.UIComponents.SnackBar.Enum.Severity.Error));
        }
    }

    /// <summary>
    /// Handles location type dropdown selection
    /// </summary>
    public async Task OnLocationTypeDDClick(DropDownEvent dropDownEvent)
    {
        var selectedLocationType = dropDownEvent.SelectionItems.FirstOrDefault();
        await _mappingViewModel.LocationTypeDDClick(selectedLocationType);
    }

    /// <summary>
    /// Handles store group type dropdown selection
    /// </summary>
    public async Task OnStoreGroupTypeDDClick(DropDownEvent dropDownEvent)
    {
        var selectedStoreGroupType = dropDownEvent.SelectionItems.FirstOrDefault();
        await _mappingViewModel.StoreGroupTypeDDClick(selectedStoreGroupType);
    }

    /// <summary>
    /// Handles deleting an item from the grid
    /// </summary>
    public void DeleteItemFromGrid(IMappingItemView mappingItemView)
    {
        mappingItemView.ActionType = Winit.Shared.Models.Enums.ActionType.Delete;
        _toast.Add("Mapping", mappingItemView.Value + _localizer["deleted_from_grid"],
            Winit.UIComponents.SnackBar.Enum.Severity.Error);
    }

    /// <summary>
    /// Saves the mapping data
    /// </summary>
    public async Task<bool> SaveMapping()
    {
        if (_mappingViewModel.MappingDTO .GridDataSource != null && _mappingViewModel.MappingDTO.GridDataSource.Any())
        {
            if (await _mappingViewModel.SaveMapping())
            {
                _toast.Add(_localizer["mapping"], _localizer["saved_successfully"],
                    Winit.UIComponents.SnackBar.Enum.Severity.Success);
                _mappingViewModel.MappingDTO.GridDataSource.RemoveAll(p =>
                    p.ActionType == Winit.Shared.Models.Enums.ActionType.Delete);
                return true;
            }
            else
            {
                _toast.Add(_localizer["error"], _localizer["failed_to_save_mapping"],
                    Winit.UIComponents.SnackBar.Enum.Severity.Error);
                return false;
            }
        }
        else
        {
            _toast.Add(_localizer["mapping"], _localizer["select_atleast_any_one"],
                Winit.UIComponents.SnackBar.Enum.Severity.Warning);
            return false;
        }
    }

    /// <summary>
    /// Gets the data grid columns configuration
    /// </summary>
    public List<DataGridColumn> GetDataGridColumns()
    {
        return new List<DataGridColumn>
        {
            new DataGridColumn
            {
                Header = _localizer["s_no"],
                GetValue = s => ((IMappingItemView)s).SNO
            },
            new DataGridColumn
            {
                Header = _localizer["type"],
                GetValue = s => ((IMappingItemView)s)?.Type
            },
            new DataGridColumn
            {
                Header = _localizer["value"],
                GetValue = s => ((IMappingItemView)s)?.Value
            },
            new DataGridColumn
            {
                Header = _localizer["isexcluded"],
                GetValue = s => ((IMappingItemView)s)?.IsExcluded
            },
            new DataGridColumn
            {
                Header = _localizer["action"],
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        ButtonType = ButtonTypes.Image,
                        URL = "Images/delete.png",
                        Action = (item) => DeleteItemFromGrid((IMappingItemView)item)
                    }
                }
            }
        };
    }
}