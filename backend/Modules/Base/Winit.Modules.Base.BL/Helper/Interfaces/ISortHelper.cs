using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.BL.Helper.Interfaces
{
    public interface ISortHelper
    {
        Task<List<T>> Sort<T>(List<T> items, Shared.Models.Enums.SortCriteria sortCriteria);
        Task<List<T>> Sort<T>(List<T> items, List<Shared.Models.Enums.SortCriteria> sortCriteriaList);
    }
}
