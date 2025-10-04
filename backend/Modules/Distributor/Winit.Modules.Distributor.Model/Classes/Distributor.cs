using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Distributor.Model.Interfaces;

namespace Winit.Modules.Distributor.Model.Classes
{
    public class Distributor:BaseModel, IDistributor
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime? OpenAccountDate { get; set; }
        public string SequenceCode { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Status { get; set; }
        public string TaxGroupUID { get; set; }
    }
}
