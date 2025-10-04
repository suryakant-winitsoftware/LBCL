using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> PagedData { get; set; }
        public int TotalCount { get; set; }
    }
}
