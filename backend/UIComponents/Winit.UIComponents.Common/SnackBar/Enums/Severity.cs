using System.ComponentModel;

namespace Winit.UIComponents.SnackBar.Enum;

public enum Severity
{
    [Description("normal")]
    Normal,
    [Description("info")]
    Info,
    [Description("success")]
    Success,
    [Description("warning")]
    Warning,
    [Description("error")]
    Error
}