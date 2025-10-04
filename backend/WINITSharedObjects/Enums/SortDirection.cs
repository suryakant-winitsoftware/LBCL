using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Enums
{
    //public enum SortDirection
    //{
    //    Asc,
    //    Desc
    //}

    public class SortCriteria
    {
        public string SortParameter { get; set; }
        public SortDirection Direction { get; set; }
    }
    public enum SortDirection
    {
        Asc,
        Desc
    }

    public class FilterCriteria
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public FilterType Type { get; set; }
    }
    public enum FilterType
    {
        NotEqual,
        Equal,
        Like
    }
}
