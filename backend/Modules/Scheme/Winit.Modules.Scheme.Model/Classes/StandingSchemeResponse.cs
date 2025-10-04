using Winit.Modules.Scheme.Model.Interfaces;
namespace Winit.Modules.Scheme.Model.Classes;

public class StandingSchemeResponse : IStandingSchemeResponse
{
    public decimal TotalAmount { get; set; }
    public List<IStandingSchemeDTO>? Schemes { get; set; }
}
