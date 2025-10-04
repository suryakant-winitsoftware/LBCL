using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Initiative.Model.Interfaces
{
    public interface IInitiativeCustomer : IBaseModel
    {
        int InitiativeCustomerId { get; set; }
        int InitiativeId { get; set; }
        string CustomerCode { get; set; }
        string CustomerName { get; set; }
        string DisplayType { get; set; }
        string DisplayLocation { get; set; }
        string ExecutionStatus { get; set; }
        string ExecutionCode { get; set; }
        int IsMulti { get; set; }
    }
}