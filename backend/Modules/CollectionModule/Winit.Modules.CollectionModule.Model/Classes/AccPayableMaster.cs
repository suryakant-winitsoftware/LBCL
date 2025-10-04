using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccPayableMaster : IAccPayableMaster
    {
        public IAccPayableCMI? AccPayableCMI {  get; set; }
        public List<IAccPayableView>? AccPayableList { get; set; }
    }
}
