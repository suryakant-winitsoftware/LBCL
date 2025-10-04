using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Contact.BL.Interfaces
{
    public interface IContactBL
    {
        Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> SelectAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
        Task<Winit.Modules.Contact.Model.Interfaces.IContact> GetContactDetailsByUID(string UID);
        Task<int> CreateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact createContact);
        Task<int> UpdateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact updateContact);
        Task<int> DeleteContactDetails(String UID);
        Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> ShowAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired);
    }
}
