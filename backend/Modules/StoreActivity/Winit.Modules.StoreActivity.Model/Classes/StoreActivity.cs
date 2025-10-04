using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.Model.Classes
{
    public class StoreActivity : BaseModel, IStoreActivity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string NavPath { get; set; }
        public int SerialNo { get; set; }
        public int IsCompulsory { get; set; }
        public int IsLocked { get; set; }
        public int IsActive { get; set; }
    }
}
