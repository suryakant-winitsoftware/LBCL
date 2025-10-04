using System;
using Winit.Modules.Base.Model;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.Model.Classes
{
    public class ExpiryCheckExecutionLine : BaseModel, IExpiryCheckExecutionLine
    {
        public int LineNumber { get; set; }
        public string SKUUID { get; set; }
        public decimal Qty { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ExpiryCheckExecutionUID { get; set; }
    }
} 