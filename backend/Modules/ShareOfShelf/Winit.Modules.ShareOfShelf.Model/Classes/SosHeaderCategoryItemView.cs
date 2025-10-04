using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.Model.Classes
{
    public class SosHeaderCategoryItemView : ISosHeaderCategoryItemView
    {
       public  string SosHeaderUID { get; set; }
       public  string ItemGroupType { get; set; }
       public  string BrandCode { get; set; }
       public  string SOSHeaderCategoryUID { get; set; }
       public  string ItemGroupUID { get; set; }
       public  string RelativePath { get; set; }
       public  string DisplayName { get; set; }
    }
}
