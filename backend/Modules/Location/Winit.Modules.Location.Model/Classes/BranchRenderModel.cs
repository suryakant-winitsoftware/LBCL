using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Location.Model.Classes
{
    public class BranchRenderModel
    {
        public bool IsStatesInformationRendered { get; set; } = true;
        public bool IsCitiesInformationRendered { get; set; }
        public bool IsLocalitiesInformationRendered { get; set; }

    }
}
