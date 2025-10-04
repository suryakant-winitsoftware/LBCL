using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tally.Model.Interfaces
{
    public interface ITallySKUMapping : IBaseModelV2
    {
        public string DistributorCode { get; set; }
        public string DistributorSKUCode { get; set; }
        public string PrincipalSKUCode { get; set; }
        public string PrincipalSKUName { get; set; }
        public string DistributorSKUName { get; set; }
        public string DistributorUOM { get; set; }
        public string PrincipleUOM { get; set; }
        public string Status { get; set; }
    }
}
