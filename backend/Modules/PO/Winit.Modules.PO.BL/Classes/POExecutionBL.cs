using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.PO.BL.Interfaces;
using Winit.Modules.PO.DL.Interfaces;
using Winit.Modules.PO.Model.Interfaces;

namespace Winit.Modules.PO.BL.Classes
{
    public class POExecutionBL : IPOExecutionBL
    {
        private readonly IPOExecutionDL _poExecutionDL;

        public POExecutionBL(IPOExecutionDL poExecutionDL)
        {
            _poExecutionDL = poExecutionDL;
        }

        public async Task<string> CreateAsync(IPOExecution poExecution)
        {
            if (!await ValidateAsync(poExecution))
            {
                throw new ArgumentException("Invalid PO execution data");
            }

            return await _poExecutionDL.CreateAsync(poExecution);
        }

        public async Task<bool> DeleteAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be empty");
            }

            return await _poExecutionDL.DeleteAsync(uid);
        }

        public async Task<IPOExecution> GetByUIDAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be empty");
            }

            return await _poExecutionDL.GetByUIDAsync(uid);
        }

        public async Task<List<IPOExecution>> GetByStoreUIDAsync(string storeUid)
        {
            if (string.IsNullOrEmpty(storeUid))
            {
                throw new ArgumentException("Store UID cannot be empty");
            }

            return await _poExecutionDL.GetByStoreUIDAsync(storeUid);
        }

        public async Task<bool> UpdateAsync(IPOExecution poExecution)
        {
            if (!await ValidateAsync(poExecution))
            {
                throw new ArgumentException("Invalid PO execution data");
            }

            return await _poExecutionDL.UpdateAsync(poExecution);
        }

        public async Task<bool> ValidateAsync(IPOExecution poExecution)
        {
            if (poExecution == null)
                return false;

            // Basic validation
            if (string.IsNullOrEmpty(poExecution.PONumber))
                return false;

            if (string.IsNullOrEmpty(poExecution.StoreUID))
                return false;

            if (string.IsNullOrEmpty(poExecution.EmpUID))
                return false;

            // Line items validation
            if (poExecution.Lines == null || !poExecution.Lines.Any())
                return false;

            // Validate each line item
            foreach (var line in poExecution.Lines)
            {
                if (string.IsNullOrEmpty(line.SKUUID))
                    return false;

                if (line.Qty <= 0)
                    return false;

                if (line.Price <= 0)
                    return false;

                if (line.TotalAmount != line.Qty * line.Price)
                    return false;
            }

            // Validate totals
            if (poExecution.LineCount != poExecution.Lines.Count)
                return false;

            if (poExecution.QtyCount != poExecution.Lines.Sum(x => x.Qty))
                return false;

            if (poExecution.TotalAmount != poExecution.Lines.Sum(x => x.TotalAmount))
                return false;

            await Task.CompletedTask; // For async consistency
            return true;
        }
    }
} 