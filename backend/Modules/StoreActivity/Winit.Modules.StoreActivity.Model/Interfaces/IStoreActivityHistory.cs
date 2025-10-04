using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.StoreActivity.Model.Interfaces
{
    public interface IStoreActivityHistory : IBaseModel
    {
        public string StoreHistoryUID { get; set; }
        public string StoreActivityUID { get; set; }
        public int SerialNo { get; set; }
        public int IsCompulsory { get; set; }
        public int IsLocked { get; set; }
        public string Status { get; set; }
    }
}
