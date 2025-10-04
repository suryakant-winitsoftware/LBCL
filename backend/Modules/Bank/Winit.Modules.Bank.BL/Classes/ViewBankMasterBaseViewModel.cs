using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Bank.BL.Interfaces;
using Winit.Modules.Bank.Model.Interfaces;

namespace Winit.Modules.Bank.BL.Classes
{
    public abstract class ViewBankMasterBaseViewModel : IViewBankMasterViewModel
    {
        public IBank ViewBankDetails { get; set; }

    }
}
