using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreCheck.Model.Interfaces;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckGroupHistory : BaseModel, IStoreCheckGroupHistory
    {
        public string StoreCheckHistoryUID { get; set; }
        public string GroupByCode { get; set; }
        public string GroupByValue { get; set; }
        public int? StoreCheckLevel { get; set; }
        public string Status { get; set; }
    }
}
