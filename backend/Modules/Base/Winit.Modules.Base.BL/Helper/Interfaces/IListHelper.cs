using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.BL.Helper.Interfaces
{
    public interface IListHelper
    {
        public Task AddItemToList<T>(
            List<T> itemList,
            T item,
            Func<T, T, bool> criteria);
    }
}
