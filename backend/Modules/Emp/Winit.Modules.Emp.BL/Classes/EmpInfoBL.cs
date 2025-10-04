using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Classes
{
    public class EmpInfoBL: IEmpInfoBL
    {
        protected readonly DL.Interfaces.IEmpInfoDL _EmpInfoBL = null;
        public EmpInfoBL(DL.Interfaces.IEmpInfoDL EmpInfoBL)
        {
            _EmpInfoBL = EmpInfoBL;
        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpInfo>> GetEmpInfoDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _EmpInfoBL.GetEmpInfoDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<Winit.Modules.Emp.Model.Interfaces.IEmpInfo> GetEmpInfoByUID(string UID)
        {
            return await _EmpInfoBL.GetEmpInfoByUID(UID);
        }
        public async Task<int> CreateEmpInfo(Winit.Modules.Emp.Model.Interfaces.IEmpInfo createEmpInfo)
        {
            return await _EmpInfoBL.CreateEmpInfo(createEmpInfo);
        }
        public async Task<int> UpdateEmpInfoDetails(Winit.Modules.Emp.Model.Interfaces.IEmpInfo updateEmpInfo)
        {
            return await _EmpInfoBL.UpdateEmpInfoDetails(updateEmpInfo);
        }
        public async Task<int> DeleteEmpInfoDetails(string Code)
        {
            return await _EmpInfoBL.DeleteEmpInfoDetails(Code);
        }
    }
}
