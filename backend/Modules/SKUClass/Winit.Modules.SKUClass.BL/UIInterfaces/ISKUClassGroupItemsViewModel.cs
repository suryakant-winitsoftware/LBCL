using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Modules.SKUClass.Model.UIInterfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.SKUClass.BL.UIInterfaces;

public interface ISKUClassGroupItemsViewModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    List<ISKU> SKUList { get; set; }
    public ISKUClassGroupMaster SKUClassGroupMaster { get; set; }
    public List<ISelectionItem> OrgSelectionItems { get; set; }
    public List<ISelectionItem> DistributionChannelSelectionItems { get; set; }
    public List<ISelectionItem> PlantSelectionItems { get; set; }
    List<ISKUAttributes> SkuAttributesList { get; set; }
    bool IsEdit { get; set; }
    Task PopulateViewModel(string skuClassGroupUID);
    Task OnOrgSelect(ISelectionItem selectionItem);
    Task BindDistributionChannelDDByOrgUID(string orgUID);
    Task AddSKUsToGrid(List<ISelectionItem> selectionItems);
    Task<bool> OnSaveClick();
}

