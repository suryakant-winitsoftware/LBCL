using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccPayableMaster
    {
        public IAccPayableCMI? AccPayableCMI { get; set; }
        public List<IAccPayableView>? AccPayableList { get; set; }
    }
}
