using Winit.Modules.CustomSKUField.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;

namespace Winit.Modules.SKU.Model.Classes;

public class SKUMaster : ISKUMaster
{
    public Winit.Modules.SKU.Model.Interfaces.ISKU SKU { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUAttributes> SKUAttributes { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUUOM> SKUUOMs { get; set; }
    public List<string> ApplicableTaxUIDs { get; set; }
    public List<Winit.Modules.SKU.Model.Interfaces.ISKUConfig> SKUConfigs { get; set; }
    public List<Winit.Modules.FileSys.Model.Interfaces.IFileSys> FileSysList { get; set; }
    public List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields> CustomSKUFields { get; set; }
    public List<CustomField> DbDataList { get; set; }

}
public class PrepareSKURequestModel
{
    public List<string> OrgUIDs { get; set; }
    public List<string> DistributionChannelUIDs { get; set; }
    public List<string> SKUUIDs { get; set; }
    public List<string> AttributeTypes { get; set; }

}
public class PrepareLinkedItemUIDModel
{
    public string LinkedItemType { get; set; }
    public List<string> StoreUIDs { get; set; }
}
public class SKUMasterData
{
    public Winit.Modules.SKU.Model.Classes.SKU SKU { get; set; }
    public List<Winit.Modules.SKU.Model.Classes.SKUAttributes> SKUAttributes { get; set; }
    public List<Winit.Modules.SKU.Model.Classes.SKUUOM> SKUUOMs { get; set; }
    public List<string> ApplicableTaxUIDs { get; set; }
    public List<Winit.Modules.SKU.Model.Classes.SKUConfig> SKUConfigs { get; set; }
    public List<Winit.Modules.SKU.Model.Classes.TaxSkuMap> TaxSKUMaps { get; set; }
    public List<Winit.Modules.CustomSKUField.Model.Classes.CustomSKUFields> CustomSKUFields { get; set; }
    public List<Winit.Modules.FileSys.Model.Classes.FileSys> FileSysList { get; set; }
    //public List<Winit.Modules.CustomSKUField.Model.Classes.CustomSKUField> customSKUField { get; set; }

}
