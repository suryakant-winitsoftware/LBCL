using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class AsmDivisionMapping : BaseModel ,IAsmDivisionMapping
    {
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string DivisionUID { get; set; }
        public string DivisionName { get; set; }
        public string StoreName { get; set; }
        public string AsmEmpUID { get; set; }
        public string AsmEmpName { get; set; }
    }
}
