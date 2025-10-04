using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.Model.Classes
{
    public class SosHeaderCategory : BaseModel , ISosHeaderCategory
    {
        public string SosHeaderUID { get; set; }
        public string ItemGroupType { get; set; }
        public string ItemGroupUID { get; set; }
        public bool IsCompleted { get; set; }
    }
}
