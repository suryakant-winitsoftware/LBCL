using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.Model.Classes
{
    public class SosLine : BaseModel, ISosLine
    {
        public string SosHeaderCategoryUID { get; set; }
        public string ItemGroupCode { get; set; }
        public int ShelvesInMeter { get; set; }
        public int ShelvesInBlock { get; set; }
        public int TotalSpace { get; set; }
    }
}
