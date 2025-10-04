using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.SKUClass.Model.Interfaces;
using Winit.Shared.Models.Enums;


namespace Winit.Modules.SKUClass.Model.Classes
{
    public class SKUClassGroup: BaseModel, ISKUClassGroup
    {
        public string CompanyUID { get; set; }
        public string SKUClassUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string FranchiseeOrgUID { get; set; }
        public bool IsActive { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string SourceType { get; set; }
        public DateTime? SourceDate { get; set; }
        public int Priority { get; set; }
        public ActionType ActionType { get; set; }
    }
}
