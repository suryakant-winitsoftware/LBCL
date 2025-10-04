using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Store.Model.Interfaces
{
    public class ApplicableToCustomerRequestBody
    {
        public List<string> Stores { get; set; } = [];
        public List<string> BroadClassifications { get; set; } = [];
        public List<string> Branches { get; set; } = [];
    }
}
