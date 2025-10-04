using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxGroupTaxes:IBaseModel
    {
        public string TaxGroupUID { get; set; }
        public string TaxUID { get; set; }
        public ActionType ActionType { get; set; }
    }
}
