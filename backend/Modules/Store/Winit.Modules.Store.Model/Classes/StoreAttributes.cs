using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreAttributes : BaseModel, IStoreAttributes
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string DistributionChannelUID { get; set; }
        public string StoreUID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string ParentName { get; set; }
    }
}
