using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes;

public class SalesPerformance : ISalesPerformance
{
    public string StoreCode { get; set; }
    public string StoreName { get; set; }
    public decimal NetAmount { get; set; }
}