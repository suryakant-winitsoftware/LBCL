using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.User.Model.Interfaces
{
    public interface IUserFranchiseeMapping
    {
        public string OrgUID { get; set; }
        public string MyOrgUID { get; set; }
        public string FranchiseeCode { get; set; }
        public string FranchiseeName { get; set; }
        public int IsFranchiseeCheck { get; set; }
        public int IsSelected { get; set; }
        
    }
}
