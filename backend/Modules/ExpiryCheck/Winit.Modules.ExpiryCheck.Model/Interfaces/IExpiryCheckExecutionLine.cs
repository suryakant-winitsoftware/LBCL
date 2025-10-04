using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ExpiryCheck.Model.Interfaces
{
    public interface IExpiryCheckExecutionLine : IBaseModel
    {
        int LineNumber { get; set; }
        string SKUUID { get; set; }
        decimal Qty { get; set; }
        DateTime? ExpiryDate { get; set; }
        string ExpiryCheckExecutionUID { get; set; }
    }
} 