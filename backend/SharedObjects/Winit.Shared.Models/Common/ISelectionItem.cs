using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Enums;

namespace Winit.Shared.Models.Common
{
    public interface ISelectionItem
    {
        public string UID { get; set; }
        public string? Code { get; set; }
        public string? Label { get; set; }
        public object? ExtData { get; set; }
        public bool IsSelected { get; set; }

        //Added to handle the items selected in drop down when click on x it should not reflect
        public bool IsSelected_InDropDownLevel { get; set; }
    }
}
