using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.PriceLadder.Model.Interfaces;

namespace Winit.Modules.PriceLadder.Model.Classes
{
    public class PriceLadderingItemView : Base.Model.BaseModel , IPriceLadderingItemView
    {
        public int? LadderingId { get; set; }
        public string OperatingUnit { get; set; }
        public string Division { get; set; }
        public string ProductCode { get; set; }
        public string ProductCategory { get; set; }
        public string Branch { get; set; }
        public string SalesOffice { get; set; }
        public decimal? ECOMPercentage { get; set; }
        public decimal? MTPercentage { get; set; }
        public decimal? LFSPercentage { get; set; }
        public decimal? DISTPercentage { get; set; }
        public decimal? RetailPercentage { get; set; }
        public decimal? SSDPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Status { get; set; }
    }
}
