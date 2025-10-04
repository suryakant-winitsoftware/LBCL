using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data
{
    public class DeviceService : IDeviceService
    {
        public string GetDeviceId()
        {
#if ANDROID
            try
            {
                // Android-specific code
                var androidId = Android.Provider.Settings.Secure.GetString(
                    Android.App.Application.Context.ContentResolver, 
                    Android.Provider.Settings.Secure.AndroidId
                );
                return androidId ?? "Unknown Android ID";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Android ID: {ex.Message}");
                return "Error";
            }
#elif WINDOWS
            // Return some Windows-specific value
            return "66a0f62f89462db6";
#else
            throw new PlatformNotSupportedException("This method is only supported on Android or Windows.");
#endif
        }
    }
}
