using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Tax.Model.Interfaces;

namespace Winit.Modules.Tax.Model.Classes
{
    public class TaxMasterDTO 
    {
        public Winit.Modules.Tax.Model.Classes.Tax Tax { get; set; }
        public List<Winit.Modules.Tax.Model.Classes.TaxSkuMap> TaxSKUMapList { get; set; }
    }
    

}
