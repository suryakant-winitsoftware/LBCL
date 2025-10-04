using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreSavedAlert: IStoreSavedAlert
    {
        public bool IsSaved { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }
    }
}
