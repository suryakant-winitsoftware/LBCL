using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.Model.Interfaces
{
    public interface ISelectionMapDetails:IBaseModel
    {
        public string? SelectionMapCriteriaUID { get; set; }
        public string? SelectionGroup { get; set; }
        public string? TypeUID { get; set; }
        public string? SelectionValue { get; set; }
        public bool IsExcluded { get; set; }
        public ActionType ActionType { get; set; }
    }
}
