using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Tax.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.UIInterfaces
{
    public interface ITaxItemView:ITax
    {
        public DateTime? ValidTo{  get; set; }
        public string CalculationType{  get; set; }
        public bool IsSelected { get; set; }
        public ActionType ActionType { get; set; }
    }
}
