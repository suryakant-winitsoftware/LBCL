using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SchemeExcludeMapping: ISchemeExcludeMapping
    {
        public long Id { get; set; }
        public string SchemeType { get; set; }
        public string SchemeUID { get; set; }
        public string StoreUID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpiredOn { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        // Added fields for import status tracking
        public bool IsValid { get; set; } 
        public string ErrorMessage { get; set; } // Only for failed records
    }
}
