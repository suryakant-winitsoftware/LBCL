using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Territory.Model.Interfaces
{
    public interface ITerritory : IBaseModel
    {
        public string OrgUID { get; set; }
        public string TerritoryCode { get; set; }
        public string TerritoryName { get; set; }
        public string ManagerEmpUID { get; set; }
        public string ClusterCode { get; set; }
        public string ParentUID { get; set; }
        public int ItemLevel { get; set; }
        public bool HasChild { get; set; }
        public bool IsImport { get; set; }
        public bool IsLocal { get; set; }
        public int IsNonLicense { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
