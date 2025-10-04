using System;
using Winit.Modules.Base.Model;
using Winit.Modules.Initiative.Model.Interfaces;

namespace Winit.Modules.Initiative.Model.Classes
{
    public class InitiativeCustomer : BaseModel, IInitiativeCustomer
    {
        public int InitiativeCustomerId { get; set; }
        public int InitiativeId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string DisplayType { get; set; }
        public string DisplayLocation { get; set; }
        public string ExecutionStatus { get; set; }
        public string ExecutionCode { get; set; }
        public int IsMulti { get; set; }
    }
}