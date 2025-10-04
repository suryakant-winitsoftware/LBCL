using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Mapping.Model.Interfaces
{
    public interface ISelectionMapMaster
    {
        public ISelectionMapCriteria? SelectionMapCriteria { get; set; }
        public List<ISelectionMapDetails>? SelectionMapDetails { get; set; }
    }
}
