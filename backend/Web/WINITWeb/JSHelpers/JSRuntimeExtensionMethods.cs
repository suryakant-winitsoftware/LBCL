using Microsoft.JSInterop;

namespace WinIt.JSHelpers
{
    public static class JSRuntimeExtensionMethods
    {
        public static async ValueTask InitializeInactivityTimer<T>(this IJSRuntime js,
            DotNetObjectReference<T> dotNetObjectReference) where T : class
        {
            await js.InvokeVoidAsync("initializeInactivityTimer",dotNetObjectReference);
        }
    }
}
