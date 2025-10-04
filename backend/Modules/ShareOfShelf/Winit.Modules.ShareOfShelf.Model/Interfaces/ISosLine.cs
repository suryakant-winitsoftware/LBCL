using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ShareOfShelf.Model.Interfaces
{
    public interface ISosLine : IBaseModel
    {
        string SosHeaderCategoryUID { get; set; }
        string ItemGroupCode { get; set; }
        int ShelvesInMeter { get; set; }
        int ShelvesInBlock { get; set; }
        int TotalSpace { get; set; }
    }
}
