using System;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Initiative.Model.Interfaces
{
    public interface IInitiative : IBaseModel
    {
        int InitiativeId { get; set; }
        string ContractCode { get; set; }
        string AllocationNo { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string SalesOrgCode { get; set; }
        string Brand { get; set; }
        decimal ContractAmount { get; set; }
        string ActivityType { get; set; }
        string DisplayType { get; set; }
        string DisplayLocation { get; set; }
        string CustomerType { get; set; }
        string CustomerGroup { get; set; }
        string PosmFile { get; set; }
        string DefaultImage { get; set; }
        string EmailAttachment { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        string Status { get; set; }
        string CancelReason { get; set; }
        bool IsActive { get; set; }
    }
}