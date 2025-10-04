using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ExpiryCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Interfaces;

namespace Winit.Modules.ExpiryCheck.BL.Interfaces
{
    public interface IExpiryCheckViewModel
    {
        public IExpiryCheckExecution expiryCheckHeader { get; set; }
        public List<ExpiryCheckItem> availableProducts { get; set; }
        Task PopulateViewModel();
        Task<string> OnSubmitExpiryCheck();
    }
}
