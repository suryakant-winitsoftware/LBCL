using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface IGroupHierarchy : IBaseModel
    {
        public string GroupTypeUID { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public string GroupParentUID { get; set; }
        public int ItemLevel { get; set; }
        public string GroupTypeName { get; set; }
        public string GroupTypeCode { get; set; }
        public string GroupTypeParentUID { get; set; }
        public int LevelNo { get; set; }
    }
}
