using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T CreateInstance<T>(this IServiceProvider serviceProvider)
        {
            try
            {
                if (serviceProvider != null)
                {
                    Type type = serviceProvider.GetRequiredService<T>().GetType();
                    object? result = Activator.CreateInstance(type);
                    return (T)result;
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
