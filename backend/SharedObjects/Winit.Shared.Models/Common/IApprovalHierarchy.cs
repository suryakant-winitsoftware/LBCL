using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public interface IApprovalHierarchy
    {
        public string ApproverId { get; set; }
        public int Level { get; set; }
        public string NextApprover { get; set; }
    }
}
