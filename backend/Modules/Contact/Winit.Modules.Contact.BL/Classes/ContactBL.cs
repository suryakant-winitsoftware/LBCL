using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Contact.BL.Classes
{
    public class ContactBL : ContactBaseBL, Interfaces.IContactBL
    {
        protected readonly DL.Interfaces.IContactDL _contactDL = null;
        public ContactBL(DL.Interfaces.IContactDL contactDL)
        {
            _contactDL = contactDL;
        }
        public async Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> SelectAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _contactDL.SelectAllContactDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
        }

        public async Task<Winit.Modules.Contact.Model.Interfaces.IContact> GetContactDetailsByUID(string UID)
        {
            return await _contactDL.GetContactDetailsByUID(UID);
        }
        public async Task<int> CreateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact createContact)
        {
            return await _contactDL.CreateContactDetails(createContact);
        }

        public async Task<int> UpdateContactDetails(Winit.Modules.Contact.Model.Interfaces.IContact updateContact)
        {
            return await _contactDL.UpdateContactDetails(updateContact);
        }

        public async Task<int> DeleteContactDetails(String UID)
        {
            return await _contactDL.DeleteContactDetails(UID);
        }
       public async Task<PagedResponse<Winit.Modules.Contact.Model.Interfaces.IContact>> ShowAllContactDetails(List<SortCriteria> sortCriterias, int pageNumber,
          int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            return await _contactDL.ShowAllContactDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);

        }
    }
}
