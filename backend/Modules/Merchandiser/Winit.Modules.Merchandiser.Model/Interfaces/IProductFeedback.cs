using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Merchandiser.Model.Interfaces
{
    public interface IProductFeedback : IBaseModel
    {
        string BeatHistoryUID { get; set; }
        string RouteUID { get; set; }
        string JobPositionUID { get; set; }
        string EmpUID { get; set; }
        DateTime ExecutionTime { get; set; }
        string StoreUID { get; set; }
        string SKUUID { get; set; }
        string FeedbackOn { get; set; }
        string EndCustomerName { get; set; }
        string MobileNo { get; set; }
    }
} 