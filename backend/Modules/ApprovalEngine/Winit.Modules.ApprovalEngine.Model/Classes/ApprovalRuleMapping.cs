using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ApprovalEngine.Model.Classes
{
    public class ApprovalRuleMapping : BaseModel, Interfaces.IApprovalRuleMapping
    {
        public int RuleId { get; set; }
        public string? Type { get; set; }
        public string? TypeCode { get; set; }
    }
}
