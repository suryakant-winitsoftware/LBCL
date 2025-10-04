using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.Model.Interfaces;

namespace Winit.Modules.Mapping.Model.Classes
{
    public class SelectionMapMasterDTO
    {
        public SelectionMapCriteria? SelectionMapCriteria { get; set; }
        public List<SelectionMapDetails>? SelectionMapDetails { get; set; }
    }
}
