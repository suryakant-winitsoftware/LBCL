using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL.Helper.Interfaces;

namespace Winit.Modules.Base.BL.Helper.Classes
{
    public class ListHelper : IListHelper
    {
        // Method to find the insertion index based on the specified list and criteria
        private async Task<int> FindInsertionIndexForItem<T>(
            List<T> itemList,
            T item,
            Func<T, T, bool> criteria)
        {
            int lastIndex = itemList.FindLastIndex(existingItem => criteria(existingItem, item));
            if (lastIndex >= 0)
            {
                return lastIndex + 1; // Insert after the last item that satisfies the criteria
            }

            // If no matching items are found, insert at the end of the list
            return itemList.Count;
        }

        // Method to add an item to the specified list next to its parent item
        /// <summary>
        /// Add/Insert Item in list.
        /// If criteria = null it will add at end
        /// If criterial != null then it will add item after the item which macthing criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemList"></param>
        /// <param name="item"></param>
        /// <param name="criteria"></param>
        public async Task AddItemToList<T>(
            List<T> itemList,
            T item,
            Func<T, T, bool> criteria)
        {
            if (itemList != null && item != null)
            {
                if (criteria == null)
                {
                    // If criteria is null, insert the item at the end of the list
                    itemList.Add(item);
                }
                else
                {
                    // Find the appropriate insertion point next to the parent item
                    int insertionIndex = await FindInsertionIndexForItem(itemList, item, criteria);

                    // Insert the item at the identified position
                    itemList.Insert(insertionIndex, item);
                }
            }
        }
    }
}
