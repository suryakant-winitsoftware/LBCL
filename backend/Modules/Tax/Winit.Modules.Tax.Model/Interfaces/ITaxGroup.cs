using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxGroup:IBaseModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public ActionType ActionType { get; set; }
    }
}
