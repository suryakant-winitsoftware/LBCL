using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.DL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Store.BL.Classes
{
    public class StoreAsmMappingBL : IStoreAsmMappingBL
    {
        private readonly IStoreAsmMappingDL _storeAsmMappingDL;
        public StoreAsmMappingBL(IStoreAsmMappingDL storeAsmMappingDL)
        {
            _storeAsmMappingDL = storeAsmMappingDL;
        }
        public async Task<PagedResponse<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping>> SelectAllStoreAsmMapping(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _storeAsmMappingDL.SelectAllStoreAsmMapping(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<List<IStoreAsmMapping>> GetExistingCustomersList(List<IStoreAsmMapping> validRecords, List<IStoreAsmMapping> invalidRecords)
        {
            List<IStoreAsmMapping> storeExistingCustomers = await _storeAsmMappingDL.GetExistingCustomersList(validRecords);

            #region 1st Validation if there is no records with combination of customercode, sitecode, division then invalid and also if IsAsmMappedByCustomer is true and it has sitecode then invalid data

            List<IStoreAsmMapping> validExistingRecords = new();
            List<IStoreAsmMapping> invalidExistingRecords = new();

            foreach (var customer in validRecords)
            {
                var matchingRecord = storeExistingCustomers.FirstOrDefault(v => v.Id == customer.Id);
                if (matchingRecord != null)
                {
                    customer.CustomerCode = matchingRecord.CustomerCode;
                    customer.StoreUID = matchingRecord.StoreUID;
                    customer.SiteUID = matchingRecord.SiteUID;
                    customer.IsAsmMappedByCustomer = matchingRecord.IsAsmMappedByCustomer;

                    if (customer.IsAsmMappedByCustomer && !string.IsNullOrEmpty(customer.SiteCode))
                    {
                        customer.ErrorMessage = "Site Code needs to be empty";
                        customer.IsValid = false;
                        invalidExistingRecords.Add(customer);
                    }
                    else
                    {
                        validExistingRecords.Add(customer);
                    }
                }
                else
                {
                    customer.ErrorMessage = "Data empty with combination of CustomerCode, SiteCode, Division";
                    customer.IsValid = false;
                    invalidExistingRecords.Add(customer);
                }
            }

            if (invalidExistingRecords.Count > 0 || invalidRecords.Count > 0)
                invalidExistingRecords.AddRange(invalidRecords);

            if (validExistingRecords.Count == 0)
                return invalidExistingRecords;
            #endregion

            List<string> EmpCodes = validExistingRecords
                     .GroupBy(e => e.EmpCode)
                     .Select(group => group.Key)
                     .ToList();

            List<IStoreAsmMapping> empExistingRecords = await _storeAsmMappingDL.GetExistingEmpList(EmpCodes);

            #region 2nd validation if there is no records with empcodes then invalid data
            List<IStoreAsmMapping> validEmpRecords = new();
            List<IStoreAsmMapping> invalidEmpRecords = new();

            foreach (var customer in validExistingRecords)
            {
                var matchingRecord = empExistingRecords.FirstOrDefault(v => v.EmpCode.Equals(customer.EmpCode, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(v.EmpUID));
                if (matchingRecord != null)
                {
                    customer.EmpCode = matchingRecord.EmpCode; 
                    customer.EmpUID = matchingRecord.EmpUID;
                    validEmpRecords.Add(customer);
                }
                else
                {
                    customer.ErrorMessage = "Invalid Emp Codes";
                    customer.IsValid = false;
                    invalidEmpRecords.Add(customer);
                }
            }

            if (invalidEmpRecords.Count > 0 || invalidExistingRecords.Count > 0)
            {
                invalidEmpRecords.AddRange(invalidExistingRecords);
            }

            if (validEmpRecords.Count == 0)
                return invalidEmpRecords;

            #endregion

            List<IStoreAsmMapping> sameBranchRecords = await _storeAsmMappingDL.CheckBranchSameOrNot(validEmpRecords);

            List<IStoreAsmMapping> validBranchRecords = new();
            List<IStoreAsmMapping> invalidBranchRecords = new();
            foreach (var customer in validEmpRecords)
            {
                var matchingRecord = sameBranchRecords.FirstOrDefault(v => v.EmpCode.Equals(customer.EmpCode, StringComparison.OrdinalIgnoreCase));
                if (matchingRecord != null)
                {
                    validBranchRecords.Add(customer);
                }
                else
                {
                    customer.ErrorMessage = "Branch is not Same";
                    customer.IsValid = false;
                    invalidBranchRecords.Add(customer);
                }
            }

            if (invalidEmpRecords.Count > 0 || invalidBranchRecords.Count > 0)
            {
                invalidBranchRecords.AddRange(invalidEmpRecords);
            }

            if (validBranchRecords.Count == 0)
                return invalidBranchRecords;

            return validBranchRecords.Concat(invalidBranchRecords).ToList();
        }
        public async Task<List<IStoreAsmMapping>> GetExistingEmpList(List<string> EmpCodes)
        {
            return await _storeAsmMappingDL.GetExistingEmpList(EmpCodes);
        }
        public async Task<int> CUAsmMapping(List<IAsmDivisionMapping> asmDivisionMapping)
        {
            return await _storeAsmMappingDL.CUAsmMapping(asmDivisionMapping);
        }
    }
}
