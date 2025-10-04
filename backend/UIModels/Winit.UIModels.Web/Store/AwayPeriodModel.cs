using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class AwayPeriodModel :BaseModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string Description { get; set; }
    }
}
