using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.BL.Interfaces;
using Winit.Modules.Emp.Model.Classes;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Emp.BL.Classes
{
    public class EmpOrgMappingBL : IEmpOrgMappingBL
    {
        protected readonly DL.Interfaces.IEmpOrgMappingDL _empOrgMappingDL;
        public EmpOrgMappingBL(DL.Interfaces.IEmpOrgMappingDL empOrgMappingDL)
        {
            _empOrgMappingDL = empOrgMappingDL;
        }
        public async Task<PagedResponse<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetails(List<SortCriteria> sortCriterias, int pageNumber,
         int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _empOrgMappingDL.GetEmpOrgMappingDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<int> CreateEmpOrgMapping(List<Winit.Modules.Emp.Model.Classes.EmpOrgMapping> empOrgMappings)
        {
            return await _empOrgMappingDL.CreateEmpOrgMapping(empOrgMappings);
        }
        public async Task<int> DeleteEmpOrgMapping(string uid)
        {
            return await _empOrgMappingDL.DeleteEmpOrgMapping(uid);
        }

        public async Task<IEnumerable<Winit.Modules.Emp.Model.Interfaces.IEmpOrgMapping>> GetEmpOrgMappingDetailsByEmpUID(string empUID)
        {
            return await _empOrgMappingDL.GetEmpOrgMappingDetailsByEmpUID(empUID);
        }
    }
}
