using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Modules.Initiative.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Initiative.DL.Interfaces
{
    public interface IInitiativeDL
    {
        // Initiative CRUD operations
        Task<IInitiative> GetInitiativeByIdAsync(int initiativeId);
        Task<IInitiative> GetInitiativeByContractCodeAsync(string contractCode);
        Task<List<IInitiative>> GetAllInitiativesAsync(InitiativeSearchRequest searchRequest);
        Task<int> GetInitiativeCountAsync(InitiativeSearchRequest searchRequest);
        Task<int> InsertInitiativeAsync(IInitiative initiative);
        Task<int> UpdateInitiativeAsync(IInitiative initiative);
        Task<bool> DeleteInitiativeAsync(int initiativeId);
        Task<string> GenerateContractCodeAsync(int initiativeId);
        Task<bool> SubmitInitiativeAsync(int initiativeId, string userCode);
        Task<bool> CancelInitiativeAsync(int initiativeId, string cancelReason, string userCode);

        // Initiative Customer operations
        Task<List<IInitiativeCustomer>> GetInitiativeCustomersAsync(int initiativeId);
        Task<bool> InsertInitiativeCustomersAsync(int initiativeId, List<IInitiativeCustomer> customers);
        Task<bool> DeleteInitiativeCustomersAsync(int initiativeId, List<string> customerCodes);
        Task<bool> UpdateInitiativeCustomerAsync(IInitiativeCustomer customer);

        // Initiative Product operations
        Task<List<IInitiativeProduct>> GetInitiativeProductsAsync(int initiativeId);
        Task<bool> InsertInitiativeProductsAsync(int initiativeId, List<IInitiativeProduct> products);
        Task<bool> DeleteInitiativeProductsAsync(int initiativeId, List<string> itemCodes);
        Task<bool> UpdateInitiativeProductAsync(IInitiativeProduct product);

        // Allocation operations
        Task<IAllocationMaster> GetAllocationByIdAsync(string allocationNo);
        Task<List<IAllocationMaster>> GetAllocationsAsync(string salesOrgCode, string brand, DateTime? startDate, DateTime? endDate);
        Task<bool> UpdateAllocationConsumedAmountAsync(string allocationNo);
        Task<decimal> GetAvailableAllocationAmountAsync(string allocationNo);

        // Validation operations
        Task<bool> ValidateContractAmountAsync(string allocationNo, decimal contractAmount, int? initiativeId = null);
        Task<bool> IsInitiativeNameUniqueAsync(string name, int? initiativeId = null);
        Task<bool> CanEditInitiativeAsync(int initiativeId);
    }
}