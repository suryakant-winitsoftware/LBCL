namespace Winit.Modules.Scheme.Model.Interfaces;

public interface IStandingSchemeDTO
{
    string SchemeUID { get; set; }
    string SchemeCode { get; set; }
    decimal Amount { get; set; }
    bool IsSelected { get; set; }
}
