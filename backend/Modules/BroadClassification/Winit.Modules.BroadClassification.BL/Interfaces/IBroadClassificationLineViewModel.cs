using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.BroadClassification.BL.Interfaces
{
    public interface IBroadClassificationLineViewModel
    {
        public List<IBroadClassificationLine> broadClassificationLinelist { get; set; }
        public IBroadClassificationLine viewBroadClassificationLineData { get; set; }
        Task PopulateViewModel();
        Task PopulateBroadClassificationLineDetailsByUID(string UID);
        Task<bool> CreateUpdateBroadClassificationLineData(IBroadClassificationLine broadClassificationline, bool Operation);
        Task<string> DeleteBroadClassificationLineData(IBroadClassificationLine broadClassificationline);
        Task ApplyFilter(List<FilterCriteria> filterCriterias);
    }
}
