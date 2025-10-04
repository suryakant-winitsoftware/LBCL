using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Mapping.BL.Interfaces;
using Winit.Modules.Mapping.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Mapping.BL.Classes
{
    public class SelectionMapDetailsBL : ISelectionMapDetailsBL
    {
        public Task<int> CreateSelectionMapDetails(List<ISelectionMapDetails> createSelectionMapDetails)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteSelectionMapDetails(List<string> UIDs)
        {
            throw new NotImplementedException();
        }

        public Task<ISelectionMapDetails> GetSelectionMapDetailsByUID(string UID)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<ISelectionMapDetails>> SelectAllSelectionMapDetails(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateSelectionMapDetails(ISelectionMapDetails updateSelectionMapDetails)
        {
            throw new NotImplementedException();
        }
    }
}
