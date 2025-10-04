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
    public class TaxItemView : Winit.Modules.Tax.Model.Classes.Tax, ITaxItemView
    {
        public DateTime? ValidTo { get; set; }
        public string CalculationType { get; set; }
        public bool IsSelected { get; set; }
        public ActionType ActionType { get; set; }
    }
}
