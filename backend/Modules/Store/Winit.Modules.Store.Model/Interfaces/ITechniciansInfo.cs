using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface ITechniciansInfo
    {
        public int Sn { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Qualification { get; set; }
    }
}
