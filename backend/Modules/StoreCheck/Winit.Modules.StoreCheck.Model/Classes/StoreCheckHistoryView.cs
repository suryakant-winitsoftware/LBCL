using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class StoreCheckHistoryView: BaseModel, IStoreCheckHistoryView
    {
        public string BeatHistoryUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string StoreAssetUID { get; set; }
        public string OrgUID { get; set; }
        public string RouteUID { get; set; }
        public string EmpUID { get; set; }
        public DateTime? StoreCheckDate { get; set; }
        public string StoreUID { get; set; }
        public string StoreCheckConfigUID { get; set; }
        public string SkuGroupTypeUID { get; set; }
        public string SkuGroupUID { get; set; }
        public string GroupByName { get; set; }
        public string Status { get; set; }
        public int? Level { get; set; }
        public bool? IsLastLevel { get; set; }
        public ActionType ActionType { get; set; }
    }
}
