using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IBouncePaymentViewModel
    {
        public AccCollectionPaymentMode[] Bank { get; set; }
        Task GetChequeDetails(string UID, string TargetUID);
    }
}
