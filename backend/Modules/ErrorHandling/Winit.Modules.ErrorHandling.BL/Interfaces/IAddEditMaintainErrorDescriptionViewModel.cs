using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IAddEditMaintainErrorDescriptionViewModel
    {
         bool IsEditErrorDescription { get; set; }
        IErrorDetailsLocalization? ErrorDetailsLocalization { get; set; }
        string ErrorDescriptionCode { get; set; }
        Task PopulateErrorDescriptionViewModel(string UID);
        Task<bool> Update();
        Task<bool> SaveOrUpdate();
    }
}
