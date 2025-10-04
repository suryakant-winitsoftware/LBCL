using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;

namespace Winit.Modules.Int_CommonMethods.Model.Classes
{
    public class IntegrationMessageProcess : BaseModel, IIntegrationMessageProcess
    {
        public long ProcessId { get; set; }
        public string InterfaceName { get; set; }
        public string MonthTableName { get; set; }
        public string TablePrefix { get; set; }
        public long? SyncLogDetailId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public int? ProcessStatus { get; set; }
        public string ErrorMessage { get; set; }
        public string ReqBatchNumber { get; set; }
    }
}
