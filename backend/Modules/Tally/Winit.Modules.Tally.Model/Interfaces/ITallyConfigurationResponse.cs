using System;
using System.Collections.Generic;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Classes;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallyConfigurationResponse 
    {
        ITallyConfiguration TallyConfigurations { get; set; }
        List<ITaxConfiguration> TaxConfigurations { get; set; }
    }
}
