using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.UIModels.Common.GridState
{
    public class TableState
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public string SortLabel { get; set; }

        public SortDirection SortDirection { get; set; }

    }
    public class TableData<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
    }
}
