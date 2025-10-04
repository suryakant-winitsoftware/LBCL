using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Winit.Shared.CommonUtilities.Common;

public static class TypeResolvers
{
    public static Action<JsonTypeInfo> GetTypeResolvers(IServiceProvider serviceProvider)
    {
        return typeInfo =>
        {
            if (typeInfo.Type.Namespace!.StartsWith("System")) return;
            //using var scope = serviceProvider.CreateScope();
            //var scopedServiceProvider = scope.ServiceProvider;
            typeInfo.CreateObject = () => serviceProvider.GetRequiredService(typeInfo.Type);
        };
    }
}
