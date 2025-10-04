using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Classes;
using Winit.Modules.Tax.Model.UIInterfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.UIClasses
{
    public class TaxGroupItemView:TaxGroup,ITaxGroupItemView
    {
        public bool IsSelected { get; set; }
    }
}
