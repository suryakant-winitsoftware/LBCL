using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
namespace Winit.Modules.ApprovalEngine.Model.Interfaces
{
    public interface IApprovalRuleMap
    {
        public string Type { get; set; }
        public string TypeCode { get; set; }
        public int RuleId { get; set; }
    }
}
