using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ErrorHandling.Model.Interfaces
{
    public interface IErrorDetailsLocalization:IBaseModel
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
