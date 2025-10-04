namespace Winit.Modules.DashBoard.Model.Interfaces;

public interface ISalesPerformance
{
    string StoreCode { get; set; }
    string StoreName { get; set; }
    decimal NetAmount { get; set; }
}