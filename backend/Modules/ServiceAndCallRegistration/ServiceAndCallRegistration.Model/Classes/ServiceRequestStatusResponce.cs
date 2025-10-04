using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ServiceAndCallRegistration.Model.Interfaces;

namespace Winit.Modules.ServiceAndCallRegistration.Model.Classes
{
    public class ServiceRequestStatusResponce : IServiceRequestStatusResponce
    {
        public int StatusCode { get; set; }
        public string CallLoggedDate { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string EquipmentNo { get; set; }
        public string EngineerName { get; set; }
        public string ServiceStatus { get; set; }
        public string ServiceOutcome { get; set; }
        public string PendingReason { get; set; }
        public string CallStatus { get; set; }
        public string ServiceCompletionDate { get; set; }
        public string CmiRelationshipNo { get; set; }
        public List<string> Errors { get; set; }
    }
}
