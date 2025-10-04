using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Merchandiser.Model.Interfaces;

namespace Winit.Modules.Merchandiser.Model.Classes
{
    public class BroadcastInitiative : BaseModel, IBroadcastInitiative
    {
        public string Gender { get; set; }
        public string EndCustomerName { get; set; }
        public string MobileNo { get; set; }
        public string FtbRc { get; set; }
        public string BeatHistoryUID { get; set; }
        public string RouteUID { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
        public DateTime ExecutionTime { get; set; }
        public string StoreUID { get; set; }
        public string SKUUID { get; set; }
    }
} 