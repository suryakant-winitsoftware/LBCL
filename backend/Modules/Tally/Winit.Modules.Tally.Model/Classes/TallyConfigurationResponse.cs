using System;
using System.Collections.Generic;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TallyConfigurationResponse : ITallyConfigurationResponse
    {
        public ITallyConfiguration TallyConfigurations { get; set; }
        public List<ITaxConfiguration> TaxConfigurations { get; set; }
    }
}
