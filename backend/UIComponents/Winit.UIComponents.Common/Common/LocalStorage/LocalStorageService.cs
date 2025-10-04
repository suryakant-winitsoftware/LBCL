using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Winit.UIComponents.Common.Common.LocalStorage
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetItem<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public async Task<T> GetItem1<T>(string key)
        {
            return await _jsRuntime.InvokeAsync<T>("localStorage.getItem", key);
        }
        public async Task<T> GetItem<T>(string key)
        {
            string jsonValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

            // Check if the retrieved value is null or empty
            if (string.IsNullOrEmpty(jsonValue))
            {
                // If the value is null or empty, return the default value for the type T
                return default;
            }

            // Deserialize the JSON value to type T
            return JsonSerializer.Deserialize<T>(jsonValue, new JsonSerializerOptions
            {
                // Ignore null values during deserialization
                IgnoreNullValues = true
            });
        }
    }
}
