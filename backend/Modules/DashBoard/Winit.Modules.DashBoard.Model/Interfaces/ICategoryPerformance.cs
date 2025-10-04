namespace Winit.Modules.DashBoard.Model.Interfaces;

public interface ICategoryPerformance
{
    string CategoryName { get; set; }
    string CategoryCode { get; set; }
    double TotalVolume { get; set; }
}