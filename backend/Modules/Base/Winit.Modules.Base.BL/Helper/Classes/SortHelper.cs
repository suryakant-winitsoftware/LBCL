using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.Base.BL.Helper.Classes
{
    public class SortHelper : ISortHelper
    {
        public async Task<List<T>> Sort<T>(List<T> items, SortCriteria sortCriteria)
        {
            List<T> sortedList = new List<T>(items);

            if (sortCriteria.Direction == SortDirection.Asc)
            {
                // Sort in ascending order
                sortedList = sortedList.OrderBy(item => item.GetPropertyValue<string>(sortCriteria.SortParameter)).ToList();
            }
            else
            {
                // Sort in descending order
                sortedList = sortedList.OrderByDescending(item => item.GetPropertyValue<string>(sortCriteria.SortParameter)).ToList();
            }

            return sortedList;
        }

        public async Task<List<T>> Sort<T>(List<T> items, List<SortCriteria> sortCriteriaList)
        {
            IOrderedEnumerable<T> sortedQuery = items.OrderBy(item => 0); // Initialize with a stable sort

            foreach (var sortCriteria in sortCriteriaList)
            {
                var criteria = sortCriteria; // Capture the criteria variable in the closure
                bool isAscending = sortCriteria.Direction == SortDirection.Asc;

                if (isAscending)
                {
                    sortedQuery = sortedQuery.ThenBy(item => item.GetPropertyValue<object>(criteria.SortParameter));
                }
                else
                {
                    sortedQuery = sortedQuery.ThenByDescending(item => item.GetPropertyValue<object>(criteria.SortParameter));
                }
            }

            return sortedQuery.ToList();
        }

    }

}
