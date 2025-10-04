using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreSavedAlert
    {
        public bool IsSaved { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }
    }
}
