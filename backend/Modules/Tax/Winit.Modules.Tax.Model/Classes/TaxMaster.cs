using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxMaster :  ITaxMaster
    {
        public Winit.Modules.Tax.Model.Interfaces.ITax Tax { get; set; }
        public List<Winit.Modules.Tax.Model.Interfaces.ITaxSkuMap> TaxSkuMapList { get; set; }
        public List<Winit.Modules.Tax.Model.Interfaces.ITaxSlab> TaxSlabList { get; set; }
    }

    


}
