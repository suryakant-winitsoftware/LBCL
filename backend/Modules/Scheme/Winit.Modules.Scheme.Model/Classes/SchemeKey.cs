using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SchemeKey: ISchemeKey
    {
        public string SchemeType { get; set; }
        public string SchemeUID { get; set; }
        public string StoreUID { get; set; }
    }
}
