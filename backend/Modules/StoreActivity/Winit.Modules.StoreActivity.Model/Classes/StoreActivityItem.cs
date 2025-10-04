using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreActivity.Model.Interfaces;

namespace Winit.Modules.StoreActivity.Model.Classes
{
    public class StoreActivityItem : IStoreActivityItem
    {
        public string UID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string NavPath { get; set; }
        public int SerialNo { get; set; }
        public int IsCompulsory { get; set; }
        public int IsLocked { get; set; }
        public string Status { get; set; }
        public string store_activity_history_uid { get; set; }
    }
}
