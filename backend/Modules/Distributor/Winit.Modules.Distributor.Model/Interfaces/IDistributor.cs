using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Distributor.Model.Interfaces
{
    public interface IDistributor:IBaseModel
    {
        string Code { get; set; }
        string Name { get; set; }
        DateTime? OpenAccountDate { get; set; }
        string SequenceCode { get; set; }
        string ContactPerson { get; set; }
        string ContactNumber { get; set; }
        string Status { get; set; }
        string TaxGroupUID { get; set; }
    }
}
