using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.Model.Classes
{
    public class TaxConfiguration : BaseModel, ITaxConfiguration
    {
        public int Id { get; set; }
        public string OrgUID { get; set; }
        public string TaxUID { get; set; }
        public string TaxName { get; set; }
        public string LEDGERNAME_CGST { get; set; }
        public string LEDGERNAME_SGST { get; set; }
        public string LEDGERNAME_ACCOUNTING { get; set; }

    }
}
