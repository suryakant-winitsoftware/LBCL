using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Int_InsertDataInQueue.Model.Interfaces
{
    public interface IApiRequest  
    {
        public string EntityName { get; set; }
        public string JsonData { get; set; }
        public string? UID { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
