using System.ComponentModel;

namespace Winit.UIComponents.SnackBar.Extensions;

internal static class EnumExtensions
{
    public static string ToDescriptionString(this System.Enum val)
    {
        var attributes = (DescriptionAttribute[])val.GetType().GetField(val.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0
            ? attributes[0].Description
            : val.ToString();
    }
}