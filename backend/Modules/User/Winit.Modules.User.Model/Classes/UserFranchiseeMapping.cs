using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.User.Model.Interfaces;

namespace Winit.Modules.User.Model.Classes
{
    public class UserFranchiseeMapping : IUserFranchiseeMapping
    {
        public string OrgUID { get; set; }
        public string MyOrgUID { get; set; }
        public string FranchiseeCode { get; set; }
        public string FranchiseeName { get; set; }
        public int IsFranchiseeCheck { get; set; }
        public int IsSelected { get; set; }

    }

    public class UserFranchiseeRequestBody
    {
        public string JobPositionUID { get; set; }
        public string OrgTypeUID { get; set; }
        public string ParentUID { get; set; }
    }
}
