namespace Winit.Modules.SKU.Model.Interfaces;

public interface ISKUFilter
{
    string SKUUID { get; set; }
    string DivisionUID { get; set; }
    HashSet<string> FilterKeys { get; set; }
}
