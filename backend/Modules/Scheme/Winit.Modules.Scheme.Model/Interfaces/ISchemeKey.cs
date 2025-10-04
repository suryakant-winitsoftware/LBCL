using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Scheme.Model.Interfaces
{
    public interface ISchemeKey
    {
        public string SchemeType { get; set; }
        public string SchemeUID { get; set; }
        public string StoreUID { get; set; }
    }
}
