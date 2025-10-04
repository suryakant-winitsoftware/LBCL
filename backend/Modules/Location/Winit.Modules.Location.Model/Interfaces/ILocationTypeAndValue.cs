using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationTypeAndValue 
    {
        public string LocationType { get; set; }
        public string LocationValue { get; set; }
    }
}
