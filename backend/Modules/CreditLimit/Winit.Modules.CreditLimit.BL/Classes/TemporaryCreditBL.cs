using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.CreditLimit.BL.Classes
{
    public class TemporaryCreditBL :  Interfaces.ITemporaryCreditBL
    {
        protected readonly DL.Interfaces.ITemporaryCreditDL _temporaryCreditDL;
        public TemporaryCreditBL(DL.Interfaces.ITemporaryCreditDL temporaryCreditDL)
        {
            _temporaryCreditDL = temporaryCreditDL;
        }
       public async  Task<PagedResponse<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit>> SelectTemporaryCreditDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired, string jobPositionUID)
        {
            return await _temporaryCreditDL.SelectTemporaryCreditDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired,jobPositionUID);
        }
     public async   Task<Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit> GetTemporaryCreditByUID(string UID)
        {
            return await _temporaryCreditDL.GetTemporaryCreditByUID(UID);
        }
         public async   Task<int> CreateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
            {
            return await _temporaryCreditDL.CreateTemporaryCreditDetails(temporaryCredit);
        }
       public async Task<int> UpdateTemporaryCreditDetails(Winit.Modules.CreditLimit.Model.Interfaces.ITemporaryCredit temporaryCredit)
        {
            return await _temporaryCreditDL.UpdateTemporaryCreditDetails(temporaryCredit);
        }
       public async Task<int> DeleteTemporaryCreditDetails(String UID)
        {
            return await _temporaryCreditDL.DeleteTemporaryCreditDetails(UID);
        }
    }
}
