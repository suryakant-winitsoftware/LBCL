using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Org.Model.Interfaces;

namespace Winit.Modules.Org.Model.Classes
{
    public class EditWareHouseItemView : BaseModel,IEditWareHouseItemView
    {
        public String UID { get; set; }
        public string WarehouseCode { get; set; }
        public string ParentUID { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseType { get; set; }
        public string FranchiseCode { get; set; }
        public string FranchiseName { get; set; }
        public string OrgTypeUID { get; set; }
        public string AddressUID { get; set; }
        public string AddressName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Landmark { get; set; }
        public string Area { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string RegionCode { get; set; }
        public string LinkedItemUID { get; set; }
        public string CountryUID { get; set; }
        public bool? ShowInUI { get; set; }
        public bool? ShowInTemplate { get; set; }

    }
}

