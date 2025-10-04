using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.BL.Interfaces
{
    public interface IOutStandingInvoicesViewModel:ITableGridViewModel
    {
        public List<IAccPayableCMI> AccPayableCMIs { get; set; }
        public IAccPayableCMI AccPayableCMIItem { get; set; }
        public IAccPayableMaster AccPayableMasterForCMIItem { get; set; }
        Task GetAccPayableCMIByUID(string uID);
        Task PopulateAccPayableCMI();
    }
}