using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.StoreActivity.Model.Interfaces
{
    public  interface IStoreActivity : IBaseModel
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
