using Winit.Modules.Location.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Location.BL.Classes
{
    public class SalesOfficeBL : ISalesOfficeBL
    {
        protected readonly DL.Interfaces.ISalesOfficeDL _salesOfficeDL;
        public SalesOfficeBL(DL.Interfaces.ISalesOfficeDL salesOfficeDL)
        {
            _salesOfficeDL = salesOfficeDL;
        }
        public async Task<PagedResponse<Winit.Modules.Location.Model.Interfaces.ISalesOffice>> SelectAllSalesOfficeDetails(List<SortCriteria> sortCriterias, int pageNumber,
             int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _salesOfficeDL.SelectAllSalesOfficeDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }
        public async Task<List<Winit.Modules.Location.Model.Interfaces.ISalesOffice>> GetSalesOfficeByUID(string UID)
        {
            return await _salesOfficeDL.GetSalesOfficeByUID(UID);
        }
        public async Task<int> CreateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            return await _salesOfficeDL.CreateSalesOffice(salesOffice);
        }
        public async Task<int> UpdateSalesOffice(Winit.Modules.Location.Model.Interfaces.ISalesOffice salesOffice)
        {
            return await _salesOfficeDL.UpdateSalesOffice(salesOffice);
        }
        public async Task<int> DeleteSalesOffice(string UID)
        {
            return await _salesOfficeDL.DeleteSalesOffice(UID);
        }
        public async Task<string?> GetWareHouseUIDbySalesOfficeUID(string salesOfficeUID)
        {
            return await _salesOfficeDL.GetWareHouseUIDbySalesOfficeUID(salesOfficeUID);
        }
    }
}
