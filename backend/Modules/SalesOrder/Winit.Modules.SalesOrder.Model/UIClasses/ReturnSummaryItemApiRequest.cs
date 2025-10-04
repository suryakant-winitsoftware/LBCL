using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SalesOrder.Model.UIClasses
{
    public class ReturnSummaryItemApiRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? StoreUID { get; set; }
        public List<FilterCriteria>? FilterCriterias { get; set; }
    }
}
