using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Org.Model.Interfaces;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Org.Model.Classes
{
    public class OrgHeirarchy : BaseModel, IOrgHeirarchy
    {
        public string UID { get; set; }
        public string OrgUID { get; set; }
        public string ParentUID { get; set; }
        public Nullable<int> ParentLevel { get; set; }
        public string Heirarchy { get; set; }

    }
}
