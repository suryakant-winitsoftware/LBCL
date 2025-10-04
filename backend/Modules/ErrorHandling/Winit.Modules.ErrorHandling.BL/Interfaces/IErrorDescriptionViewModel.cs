using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ErrorHandling.Model.Interfaces;

namespace Winit.Modules.ErrorHandling.BL.Interfaces
{
    public interface IErrorDescriptionViewModel
    {
        public IErrorDescriptionDetails ErrorDescriptionDetails { get; set; }
        Task PopulateErrorDescriptionDetailsByUID(string UID);

    }
}
