using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKU.Model.UIClasses;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Events;

namespace Winit.Modules.SKUClass.BL.UIInterfaces;

public interface ISKUClassGroupItemsViewModelV1
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    List<ISKUV1> SKUs { get; set; }
    public ISKUClassGroupMaster SKUClassGroupMaster { get; set; }
    public List<ISelectionItem> OrgSelectionItems { get; set; }
    public List<ISelectionItem> DistributionChannelSelectionItems { get; set; }
    public List<ISelectionItem> PlantSelectionItems { get; set; }
    public List<SKUAttributeDropdownModel> SKUAttributeData { get; set; }
    public string SKUSearchString { get; set; }
    bool IsEdit { get; set; }
    List<ISelectionItem> BroadClassificationSelectionItems { get; set; }
    List<ISelectionItem> BranchDdlSelectionItems { get; set; }
    List<ISelectionItem> ChannelPartners { get; set; }
    IEnumerable<ISKUV1> DisplayGridSKUs { get; }
    List<ISKUV1> GridSKUs { get; set; }
    List<string> SkuExcludeList { get; set; }
    List<string> SkuDeleteList { get; set; }
    List<ISelectionItem> SelectedBranches { get; set; }
    List<ISelectionItem> SelectedCP { get; set; }
    List<ISelectionItem> SelectedBC { get; set; }
    Task PopulateViewModel(string skuClassGroupUID);
    Task OnOrgSelect(ISelectionItem selectionItem);
    Task BindDistributionChannelDDByOrgUID(string orgUID);
    Task AddSKUsToGrid(List<ISKUV1> selectionItems);
    bool FilterAction(List<FilterCriteria> filterCriterias, ISKUV1 sKUV1);
    Task<List<ISelectionItem>> OnSKuAttributeDropdownValueSelect(string selectedItemUID);
    Task OnBranchSelected(DropDownEvent dropDownEvent);
    Task PopulateApplicableToCustomersAndSKU();
    void OnChannelpartnerSelected(DropDownEvent dropDownEvent);
    void OnBroadClassificationSelected(DropDownEvent dropDownEvent);
    void AddProductsToGrid(List<ISKUV1> selectionItems);
    Task<ApiResponse<string>> OnSaveClick();
}
