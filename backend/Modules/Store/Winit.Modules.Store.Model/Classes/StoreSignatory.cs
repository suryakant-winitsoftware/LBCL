using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreSignatory:IStoreSignatory
    {
        public int? Sn { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PanNo { get; set; }
        public string ODLimit { get; set; }
    }
}
