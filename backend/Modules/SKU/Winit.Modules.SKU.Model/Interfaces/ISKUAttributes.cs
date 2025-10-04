using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.SKU.Model.Interfaces
{
    public interface ISKUAttributes : IBaseModel
    {
        public string SKUUID { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string ParentType { get; set; }
        public ActionType ActionType { get; set; }
    }
}
