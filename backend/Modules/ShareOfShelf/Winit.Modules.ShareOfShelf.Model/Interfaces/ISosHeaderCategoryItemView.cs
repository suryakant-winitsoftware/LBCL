using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.ShareOfShelf.Model.Interfaces
{
    public interface ISosHeaderCategoryItemView
    {
        string SosHeaderUID { get; set; }
        string ItemGroupType { get; set; }
        string BrandCode { get; set; } 
        string SOSHeaderCategoryUID { get; set; } 
        string ItemGroupUID { get; set; } 
        string RelativePath { get; set; } 
        string DisplayName { get; set; }
    }
}
