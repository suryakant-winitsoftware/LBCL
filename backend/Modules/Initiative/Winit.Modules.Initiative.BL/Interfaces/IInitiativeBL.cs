using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Modules.Initiative.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Initiative.BL.Interfaces
{
    public interface IInitiativeBL
    {
        // Initiative operations
        Task<InitiativeDTO> GetInitiativeByIdAsync(int initiativeId);
        Task<PagedResponse<InitiativeDTO>> GetInitiativesAsync(InitiativeSearchRequest searchRequest);
        Task<InitiativeDTO> CreateInitiativeAsync(CreateInitiativeRequest request, string userCode);
        Task<InitiativeDTO> UpdateInitiativeAsync(int initiativeId, CreateInitiativeRequest request, string userCode);
        Task<bool> DeleteInitiativeAsync(int initiativeId, string userCode);
        Task<InitiativeDTO> SubmitInitiativeAsync(int initiativeId, string userCode);
        Task<bool> CancelInitiativeAsync(int initiativeId, string cancelReason, string userCode);
        Task<InitiativeDTO> SaveDraftAsync(int initiativeId, CreateInitiativeRequest request, string userCode);
        
        // Customer operations
        Task<bool> UpdateInitiativeCustomersAsync(int initiativeId, List<InitiativeCustomerDTO> customers, string userCode);
        Task<List<InitiativeCustomerDTO>> GetInitiativeCustomersAsync(int initiativeId);
        
        // Product operations
        Task<bool> UpdateInitiativeProductsAsync(int initiativeId, List<InitiativeProductDTO> products, string userCode);
        Task<List<InitiativeProductDTO>> GetInitiativeProductsAsync(int initiativeId);
        
        // Allocation operations
        Task<List<AllocationMasterDTO>> GetAvailableAllocationsAsync(string salesOrgCode, string brand, DateTime? startDate, DateTime? endDate);
        Task<AllocationMasterDTO> GetAllocationDetailsAsync(string allocationNo);
        Task<bool> ValidateAllocationAmountAsync(string allocationNo, decimal contractAmount, int? initiativeId = null);
        
        // File operations
        Task<string> UploadFileAsync(int initiativeId, string fileType, byte[] fileContent, string fileName, string userCode);
        Task<bool> DeleteFileAsync(int initiativeId, string fileType, string userCode);
        
        // Validation operations
        Task<ValidationResult> ValidateInitiativeAsync(CreateInitiativeRequest request, int? initiativeId = null);
        Task<bool> CanEditInitiativeAsync(int initiativeId, string userCode);
        Task<bool> CanDeleteInitiativeAsync(int initiativeId, string userCode);
    }
    
    public class AllocationMasterDTO
    {
        public string AllocationNo { get; set; }
        public string ActivityNo { get; set; }
        public string AllocationName { get; set; }
        public string AllocationDescription { get; set; }
        public decimal TotalAllocationAmount { get; set; }
        public decimal AvailableAllocationAmount { get; set; }
        public decimal ConsumedAmount { get; set; }
        public string Brand { get; set; }
        public string SalesOrgCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? DaysLeft { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}