using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.Models.Common
{
    public class PagingRequest
    {
        public List<SortCriteria>? SortCriterias { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<FilterCriteria>? FilterCriterias { get; set; }
        public bool IsCountRequired { get; set; }
    }
}
