using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCustomer
    {
        public string Code { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
    }
}
