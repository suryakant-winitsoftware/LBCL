using System;
using System.Collections.Generic;

namespace Winit.Modules.Initiative.Model.Classes
{
    public class InitiativeDTO
    {
        public int InitiativeId { get; set; }
        public string ContractCode { get; set; }
        public string AllocationNo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SalesOrgCode { get; set; }
        public string Brand { get; set; }
        public decimal ContractAmount { get; set; }
        public string ActivityType { get; set; }
        public string DisplayType { get; set; }
        public string DisplayLocation { get; set; }
        public string CustomerType { get; set; }
        public string CustomerGroup { get; set; }
        public string PosmFile { get; set; }
        public string DefaultImage { get; set; }
        public string EmailAttachment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string CancelReason { get; set; }
        public bool IsActive { get; set; }
        public List<InitiativeCustomerDTO> Customers { get; set; }
        public List<InitiativeProductDTO> Products { get; set; }
    }

    public class InitiativeCustomerDTO
    {
        public int InitiativeCustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string DisplayType { get; set; }
        public string DisplayLocation { get; set; }
        public string ExecutionStatus { get; set; }
    }

    public class InitiativeProductDTO
    {
        public int InitiativeProductId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public decimal? PttPrice { get; set; }
    }

    public class CreateInitiativeRequest
    {
        public string AllocationNo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SalesOrgCode { get; set; }
        public string Brand { get; set; }
        public decimal ContractAmount { get; set; }
        public string ActivityType { get; set; }
        public string DisplayType { get; set; }
        public string DisplayLocation { get; set; }
        public string CustomerType { get; set; }
        public string CustomerGroup { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> CustomerCodes { get; set; }
        public List<InitiativeProductDTO> Products { get; set; }
    }

    public class InitiativeSearchRequest
    {
        public string SearchText { get; set; }
        public string SalesOrgCode { get; set; }
        public string Brand { get; set; }
        public string Status { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}