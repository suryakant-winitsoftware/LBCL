using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxGroup:BaseModel,ITaxGroup
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public ActionType ActionType { get; set; }
    }
}
