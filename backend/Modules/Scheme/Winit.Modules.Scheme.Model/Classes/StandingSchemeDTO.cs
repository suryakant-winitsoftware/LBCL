using Winit.Modules.Scheme.Model.Interfaces;
namespace Winit.Modules.Scheme.Model.Classes;

public class StandingSchemeDTO : IStandingSchemeDTO
{
    public string SchemeUID { get; set; }
    public string SchemeCode { get; set; }
    public decimal Amount { get; set; }
    public bool IsSelected { get; set; } = true;
}
