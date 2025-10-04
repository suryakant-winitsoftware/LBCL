using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class DataGridColumn
    {
        public ColumnTypes ColumnType { get; set; } = ColumnTypes.Text;
        public string Header { get; set; }
        public Func<object, object> GetValue { get; set; }
        public bool IsButtonColumn { get; set; }
        public bool IsTextBox { get; set; } = false;
        public bool IsCheckBoxColumn { get; set; }
        public List<ButtonAction> ButtonActions { get; set; }
        public bool IsSortable { get; set; } // Indicates if the column is sortable
        public string SortField { get; set; } // Field used for sorting
        public string Style { get; set; } // Field used for sorting
        public string Class { get; set; } // Field used for sorting
        public Func<object, bool> ConditionalVisibility { get; set; } = item => true;
        //public bool IsImageColumn { get; set; } = false;
        public Action<object> Action { get; set; }

    }
    public class ButtonAction
    {
        public string Text { get; set; }
        public string URL { get; set; }
        public ButtonTypes ButtonType { get; set; }
        public bool IsDisabled { get; set; }
        public Action<object> Action { get; set; }

        public Func<object, object> GetValue { get; set; }
        public Func<object, bool> ConditionalVisibility { get; set; } = item => true;
        public bool IsVisible { get; set; } = true;
    }
    public enum ButtonTypes
    {
        Text, Image, Url, CheckBox
    }
    public enum ColumnTypes
    {
        Text, Image, Url, CheckBox
    }
}
