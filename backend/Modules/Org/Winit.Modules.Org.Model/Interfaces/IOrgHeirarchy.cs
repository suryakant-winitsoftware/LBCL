using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IOrgHeirarchy
    {

        public string UID { get; set; }
        public string OrgUID { get; set; }
        public string ParentUID { get; set; }
        public Nullable<int> ParentLevel { get; set; }
        public string Heirarchy { get; set; }
    }
}
