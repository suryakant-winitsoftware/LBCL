using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreAttributes : Base.Model.IBaseModel
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
