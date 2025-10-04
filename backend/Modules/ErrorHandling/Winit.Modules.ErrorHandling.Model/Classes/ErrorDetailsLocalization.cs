using Winit.Modules.Base.Model;
using Winit.Modules.ErrorHandling.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.Model.Classes
{
    public class ErrorDetailsLocalization : BaseModel,IErrorDetailsLocalization
    {
        public string ErrorCode { get; set; }
        public string LanguageCode { get; set; }
        public string Description { get; set; }
        public string Cause { get; set; }
        public string Resolution { get; set; }
        public string ShortDescription { get; set; }
        public ActionType ActionType { get; set; }
    }
}
