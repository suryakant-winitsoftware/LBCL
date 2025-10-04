using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_InsertDataInQueue.Model.Interfaces;

namespace Winit.Modules.Int_InsertDataInQueue.Model.Classes
{
    public class ApiRequest : IApiRequest
    {
        public string EntityName { get; set; }
        public string JsonData { get; set; }
        //public string entity_name { get; set; }
        //public string Content { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public string? UID { get; set; }
    }
}
