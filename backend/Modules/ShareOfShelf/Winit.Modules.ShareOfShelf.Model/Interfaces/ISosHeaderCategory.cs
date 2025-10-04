using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ShareOfShelf.Model.Interfaces
{
    public interface ISosHeaderCategory : IBaseModel
    {
         string SosHeaderUID { get; set; }
         string ItemGroupType { get; set; }
         string ItemGroupUID { get; set; }
         bool IsCompleted { get; set; }
    }
}
