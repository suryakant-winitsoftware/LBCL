using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.BroadClassification.BL.Interfaces;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.BL.Classes
{
    public abstract class BroadClassificationLineBaseViewModel : IBroadClassificationLineViewModel
    {
        private readonly IFilterHelper _filter;
        private readonly ISortHelper _sorter;
        private readonly IListHelper _listHelper;
        public Winit.Shared.Models.Common.IAppConfig _appConfigs;
        private Winit.Modules.Base.BL.ApiService _apiService;
        private IServiceProvider _serviceProvider;
        public List<FilterCriteria> broadClassificationLineDataFilterCriterias { get; set; }

        public List<IBroadClassificationLine> broadClassificationLinelist { get; set; }
        public IBroadClassificationLine viewBroadClassificationLineData { get; set; }

        public BroadClassificationLineBaseViewModel(IServiceProvider serviceProvider,
           IFilterHelper filter,
           ISortHelper sorter,
           IListHelper listHelper, Winit.Shared.Models.Common.IAppConfig appConfigs, Winit.Modules.Base.BL.ApiService apiService
         )
        {
            _serviceProvider = serviceProvider;
            _filter = filter;
            _sorter = sorter;
            _listHelper = listHelper;
            _apiService = apiService;
            _appConfigs = appConfigs;
            broadClassificationLinelist = new List<IBroadClassificationLine>();
        }


        public async virtual Task PopulateViewModel()
        {
            await GetBroadClassificationLinedata();
        }

        public async virtual Task PopulateBroadClassificationLineDetailsByUID(string UID)
        {
            viewBroadClassificationLineData = await GetBroadClassificationLineDetailsByUID(UID);
        }

        private async Task GetBroadClassificationLinedata()
        {
            broadClassificationLinelist = await GetBroadClassificationLineData();
            broadClassificationLinelist = broadClassificationLinelist.GroupBy(item => item.ClassificationCode)
                                                    .Select(group => group.First())
                                                    .ToList();
        }
        public async Task ApplyFilter(List<FilterCriteria> filterCriterias)
        {
            broadClassificationLineDataFilterCriterias.Clear();
            broadClassificationLineDataFilterCriterias.AddRange(filterCriterias);
            await GetBroadClassificationLinedata();
        }
        public abstract Task<List<IBroadClassificationLine>> GetBroadClassificationLineData();
        public abstract Task<IBroadClassificationLine> GetBroadClassificationLineDetailsByUID(string UID);
        public abstract Task<bool> CreateUpdateBroadClassificationLineData(IBroadClassificationLine broadClassificationLine, bool Operation);
        public abstract Task<string> DeleteBroadClassificationLineData(IBroadClassificationLine broadClassificationline);

    }
}
