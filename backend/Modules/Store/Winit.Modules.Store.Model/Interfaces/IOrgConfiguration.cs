using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Classes;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IOrgConfiguration
    {
        List<IStoreCredit> StoreCredit { get; set; }
        List<IStoreAttributes> StoreAttributes { get; set; }
    }
}
