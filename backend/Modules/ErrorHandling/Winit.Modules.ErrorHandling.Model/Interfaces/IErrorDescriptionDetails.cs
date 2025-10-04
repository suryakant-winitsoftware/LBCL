using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ErrorHandling.Model.Interfaces
{
    public interface IErrorDescriptionDetails
    { 
        public IErrorDetailModel errorDetail { get; set; }
        public List<IErrorDetailsLocalization> errorDetailsLocalizationList { get; set; }

    }
}
