using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreViewModel : StoreBaseViewModel
    {

        public StoreViewModel(IStoreBL storeBL):base(storeBL) { }
    }
}
