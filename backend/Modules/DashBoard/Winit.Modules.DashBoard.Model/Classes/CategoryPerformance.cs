using Winit.Modules.DashBoard.Model.Interfaces;

namespace Winit.Modules.DashBoard.Model.Classes;

public class CategoryPerformance:ICategoryPerformance
{
    public string CategoryName { get; set; }
    public string CategoryCode { get; set; }
    public double TotalVolume { get; set; }
}