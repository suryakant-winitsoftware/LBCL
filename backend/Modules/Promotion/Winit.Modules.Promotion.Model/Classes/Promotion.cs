using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Promotion.Model.Interfaces;

namespace Winit.Modules.Promotion.Model.Classes
{
    public class Promotion : BaseModel, IPromotion
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string Category { get; set; }
        public bool HasSlabs { get; set; }
        public string CreatedByEmpUID { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpto { get; set; }
        public string Type { get; set; }
        public string PromoFormat { get; set; }
        public string PromoFormatLabel { get; set; }
        public bool IsActive { get; set; }
        public string PromoTitle { get; set; }
        public string PromoMessage { get; set; }
        public string Status { get; set; }
        public bool HasFactSheet { get; set; }
        public int Priority { get; set; }
        public decimal ContributionLevel1 { get; set; }
        public decimal ContributionLevel2 { get; set; }
        public decimal ContributionLevel3 { get; set; }
        public bool IsApprovalCreated { get; set; }

    }
}
