using Winit.Modules.Scheme.Model.Classes;

namespace Winit.Modules.Scheme.Model.Interfaces;

public interface IStandingSchemeResponse
{
    decimal TotalAmount { get; set; }
    List<IStandingSchemeDTO>? Schemes { get; set; }
}
