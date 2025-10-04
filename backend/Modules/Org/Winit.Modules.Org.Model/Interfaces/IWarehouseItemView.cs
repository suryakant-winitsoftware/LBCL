using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Org.Model.Interfaces
{
    public interface IWarehouseItemView:IBaseModel
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
