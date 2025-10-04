using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.BL.Helper.Interfaces
{
    public interface IFilterHelper
    {
        Task<bool> MatchFilter<T>(T item, Shared.Models.Enums.FilterCriteria filterCriteria);
        Task<List<T>> ApplyFilter<T>(List<T> sourceList, List<Shared.Models.Enums.FilterCriteria> filterCriteriaList, Shared.Models.Enums.FilterMode filterMode);
        Task<List<T>> ApplySearch<T>(List<T> sourceList, string searchString, List<string> propertiesToSearch);
        Task<List<T1>> FilterFromList<T1, U>(
            IEnumerable<T1> items,
            Func<T1, IEnumerable<U>> attributeListSelector,
            List<(Func<U, string> fieldSelector, string fieldSelectorValue)> filterCriteria);
        Task<List<T1>> FilterFromDictionary<T1, U>(
            IEnumerable<T1> items,
            Func<T1, Dictionary<string, U>> attributeListSelector,
            List<(Func<U, string> fieldSelector, string fieldSelectorValue)> filterCriteria,
            string key = null);
    }
}
