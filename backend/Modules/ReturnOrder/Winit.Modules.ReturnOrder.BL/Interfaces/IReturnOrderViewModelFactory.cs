using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ReturnOrder.BL.Interfaces
{
    public interface IReturnOrderViewModelFactory
    {
        IReturnOrderViewModel? CreateReturnOrderViewModel(string orderType, string source);
    }
}
