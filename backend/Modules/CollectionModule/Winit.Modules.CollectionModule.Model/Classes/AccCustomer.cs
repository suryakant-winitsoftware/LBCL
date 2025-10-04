using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccCustomer : IAccCustomer
    {
        public string Code { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
    }
}
