using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxDependencies: IBaseModel
    {
        public string TaxUID { get; set; }
        public string DependsOnTaxUID { get; set; }
    }
}
