using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.Tally.BL.Interfaces;
using Winit.Modules.Tally.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Tally.BL.Classes.TallySKUMapping
{
    public abstract class TallySKUMappingBaseViewModel : ITallySKUMappingViewModel
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IAppConfig _appConfig;
        protected readonly IAppUser _appUser;
        public List<FilterCriteria> FilterCriterias { get; set; } = new List<FilterCriteria>();
        public List<SortCriteria> SortCriterias { get; set; } = new List<SortCriteria>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;
        public TallySKUMappingBaseViewModel(IServiceProvider serviceProvider, IAppConfig appConfig, IAppUser appUser)
        {
            _serviceProvider = serviceProvider;
            _appUser = appUser;
            _appConfig = appConfig;
        }

        public abstract Task<List<ITallySKUMapping>> GetAllSKUMappingDetailsByDistCode(string Code, string Tab);
        public abstract Task<List<ITallySKU>> GetAllTallySKUDetails();
        public abstract Task<List<ISKUV1>> GetAllSKUDetailsByOrgUID(string Code);
        public abstract Task<bool> InsertTallySKUMapping(List<ITallySKUMapping> tallySKUMapping);
        public abstract Task<List<IEmp>> GetAllDistributors();
    }
}
