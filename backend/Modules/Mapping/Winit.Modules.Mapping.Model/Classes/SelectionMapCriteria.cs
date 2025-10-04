using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.Model.Classes
{
    public class SelectionMapCriteria:BaseModel,ISelectionMapCriteria
    {
        public string? LinkedItemUID { get; set; }
        public string? LinkedItemType { get; set; }
        public bool HasOrganization { get; set; }
        public bool HasLocation { get; set; }
        public bool HasCustomer { get; set; }
        public bool HasSalesTeam { get; set; }
        public bool HasItem { get; set; }
        public int OrgCount { get; set; }
        public int LocationCount { get; set; }
        public int CustomerCount { get; set; }
        public int SalesTeamCount { get; set; }
        public int ItemCount { get; set; }
        public ActionType ActionType { get; set; }
        public bool IsActive { get; set; }
    }
}
