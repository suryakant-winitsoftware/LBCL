using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.Model.Interfaces;

namespace Winit.Modules.Org.Model.Classes
{
    public class WarehouseItemView: BaseModel, IWarehouseItemView
    {
        public int Id { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseUID { get; set; }
        public string WarehouseName { get; set; }
        public string FranchiseCode { get; set; }
        public string FranchiseName { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string OrgTypeUID { get; set; }
    }
}

