using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SchemeOrg : BaseModel, ISchemeOrg
    {
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string OrgUID { get; set; }
    }
}
