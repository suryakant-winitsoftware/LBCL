using System;
using System.Collections.Generic;
using Winit.Modules.Base.Model;
using Winit.Modules.Initiative.Model.Interfaces;

namespace Winit.Modules.Initiative.Model.Classes
{
    public class Initiative : BaseModel, IInitiative
    {
        public Initiative()
        {
            InitiativeCustomers = new List<InitiativeCustomer>();
            InitiativeProducts = new List<InitiativeProduct>();
        }

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

        // Navigation properties
        public AllocationMaster AllocationMaster { get; set; }
        public List<InitiativeCustomer> InitiativeCustomers { get; set; }
        public List<InitiativeProduct> InitiativeProducts { get; set; }
    }
}