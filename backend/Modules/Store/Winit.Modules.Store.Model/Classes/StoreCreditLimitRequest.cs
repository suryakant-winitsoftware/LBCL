namespace Winit.Modules.Store.Model.Classes;

public class StoreCreditLimitRequest
{
    public required List<string>  StoreUids { get; set; }
    public string? DivisionUID { get; set; }
}
