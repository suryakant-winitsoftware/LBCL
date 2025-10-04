using Winit.Modules.SKU.Model.Interfaces;
namespace Winit.Modules.SKU.Model.Classes;

public class SKUFilter : ISKUFilter
{
    public string SKUUID { get; set; }
    public string DivisionUID { get; set; }
    public HashSet<string> FilterKeys { get; set; }
}
