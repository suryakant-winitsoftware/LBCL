using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Interfaces;

namespace Winit.Modules.Mapping.Model.Classes
{
    public class SelectionMapMaster:ISelectionMapMaster
    {
        public ISelectionMapCriteria? SelectionMapCriteria { get; set; }
        public List<ISelectionMapDetails>? SelectionMapDetails { get; set; }
    }
}
