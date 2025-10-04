using Microsoft.AspNetCore.Components;
using Winit.UIComponents.SnackBar.Enum;
using Winit.UIComponents.SnackBar.Services;

namespace Winit.UIComponents.SnackBar;

public interface IToast : IDisposable
{
    IEnumerable<Services.Toast> ShownToasts { get; }

    ToastConfiguration Configuration { get; }

    event Action OnToastsUpdated;

    Services.Toast Add(string title,string message, Severity severity = Severity.Normal, Action<ToastOptions> configure = null, string key = "");
    Services.Toast Add(string title, RenderFragment message, Severity severity = Severity.Normal, Action<ToastOptions> configure = null, string key = "");
    Services.Toast Add<T>(Dictionary<string, object> componentParameters = null, Severity severity = Severity.Normal, Action<ToastOptions> configure = null, string key = "") where T : IComponent;

    void Clear();

    void Remove(Services.Toast snackbar);
}