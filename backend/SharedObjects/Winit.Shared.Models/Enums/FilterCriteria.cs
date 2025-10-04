using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Enums
{
    public class FilterCriteria
    {
        public string Name { get; set; }
        public object? Value { get; set; }
        public FilterType Type { get; set; }
        public Type DataType { get; set; } = typeof(string);
        public FilterGroupType FilterGroup { get; set; } = FilterGroupType.Field;
        public FilterMode FilterMode { get; set; }
        public FilterCriteria(string name, object? value, FilterType type, Type? dataType = null, FilterGroupType filterGroup = FilterGroupType.Field,FilterMode filterMode = FilterMode.And)
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
            if(dataType != null)
            {
                DataType = dataType;
            }
            FilterGroup = filterGroup;
            FilterMode = filterMode;
        }
    }
}
