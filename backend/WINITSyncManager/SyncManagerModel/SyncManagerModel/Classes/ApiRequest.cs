using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncManagerModel.Interfaces;

namespace SyncManagerModel.Classes
{
    public class ApiRequest : IApiRequest
    {
        public string? EntityName { get; set; }
        public string? JsonData { get; set; } 
        public string? UID { get; set; }
        public string? Status { get ; set ; }
        public string? Remarks { get ; set ; }
    }
}
