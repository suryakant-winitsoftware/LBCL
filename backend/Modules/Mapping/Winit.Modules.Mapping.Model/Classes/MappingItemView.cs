using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.Model.Classes
{
    public class MappingItemView:BaseModel, IMappingItemView
    {
        public string? UID { get; set; }
        public string? TypeUID { get; set; }
        public string? TypeCode { get; set; }
        public string? Code { get; set; }
        public int SNO { get; set; }
        public bool IsExcluded { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public string? Group { get; set; }
        public ActionType ActionType { get; set; }
    }
}
