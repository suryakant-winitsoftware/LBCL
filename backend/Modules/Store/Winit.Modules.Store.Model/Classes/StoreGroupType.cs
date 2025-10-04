using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreGroupType : BaseModel, IStoreGroupType
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public string Code { get; set; }
        public int LevelNo { get; set; }
    }
}
