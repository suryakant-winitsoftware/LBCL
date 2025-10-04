using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.UIInterfaces
{
    public interface ITaxGroupItemView:ITaxGroup
    {
        public bool IsSelected { get; set; }
    }
}
