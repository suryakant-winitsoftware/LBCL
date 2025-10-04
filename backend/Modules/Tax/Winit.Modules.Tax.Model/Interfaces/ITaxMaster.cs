using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Winit.Modules.Tax.Model.Interfaces
{
    public interface ITaxMaster 
    {
        public Winit.Modules.Tax.Model.Interfaces.ITax Tax { get; set; }
        public List<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap>  TaxSkuMapList { get; set; }
        public List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab> TaxSlabList { get; set; }


    }
}
