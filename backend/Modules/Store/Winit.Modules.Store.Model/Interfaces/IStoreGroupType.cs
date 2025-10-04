using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreGroupType:IBaseModel
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
