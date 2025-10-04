using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.Model.Classes
{
    public class ErrorDescriptionDetails : IErrorDescriptionDetails
    {
        public IErrorDetailModel errorDetail { get; set; }
        public List<IErrorDetailsLocalization> errorDetailsLocalizationList { get; set; }
    }

    public class ErrorDescriptionDetailsDTO
    {
        public ErrorDetailModel? errorDetail { get; set; }
        public List<ErrorDetailsLocalization>? errorDetailsLocalizationList { get; set; }
    }
}
