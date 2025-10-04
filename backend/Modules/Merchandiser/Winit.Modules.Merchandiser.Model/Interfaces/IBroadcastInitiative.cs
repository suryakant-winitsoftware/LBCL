using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Merchandiser.Model.Interfaces
{
    public interface IBroadcastInitiative : IBaseModel
    {
        string Gender { get; set; }
        string EndCustomerName { get; set; }
        string MobileNo { get; set; }
        string FtbRc { get; set; }
        string BeatHistoryUID { get; set; }
        string RouteUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        DateTime ExecutionTime { get; set; }
        string StoreUID { get; set; }
        string SKUUID { get; set; }
    }
} 