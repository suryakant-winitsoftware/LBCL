using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;

namespace Winit.Modules.Tally.BL.Interfaces
{
    public interface ITallySKUMappingViewModel
    {
        public int PageNumber { get; set; } 
        public int PageSize { get; set; } 
        public int TotalCount { get; set; }
        Task<List<ITallySKUMapping>> GetAllSKUMappingDetailsByDistCode(string Code ,string Tab);
        Task<List<ITallySKU>> GetAllTallySKUDetails();
        Task<List<ISKUV1>> GetAllSKUDetailsByOrgUID(string Code);
        Task<bool> InsertTallySKUMapping(List<ITallySKUMapping> tallySKUMapping);
        Task <List<IEmp>> GetAllDistributors();
    }
}
