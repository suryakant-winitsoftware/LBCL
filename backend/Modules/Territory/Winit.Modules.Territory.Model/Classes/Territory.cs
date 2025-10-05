using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Territory.Model.Interfaces;

namespace Winit.Modules.Territory.Model.Classes
{
    public class Territory : BaseModel, ITerritory
    {
        [Column("org_uid")]
        public string OrgUID { get; set; }

        [Column("territory_code")]
        public string TerritoryCode { get; set; }

        [Column("territory_name")]
        public string TerritoryName { get; set; }

        [Column("manager_emp_uid")]
        public string ManagerEmpUID { get; set; }

        [Column("cluster_code")]
        public string ClusterCode { get; set; }

        [Column("parent_uid")]
        public string ParentUID { get; set; }

        [Column("item_level")]
        public int ItemLevel { get; set; }

        [Column("has_child")]
        public bool HasChild { get; set; }

        [Column("is_import")]
        public bool IsImport { get; set; }

        [Column("is_local")]
        public bool IsLocal { get; set; }

        [Column("is_non_license")]
        public int IsNonLicense { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
