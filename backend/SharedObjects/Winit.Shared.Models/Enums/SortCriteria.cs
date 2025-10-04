using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Converters;

namespace Winit.Shared.Models.Enums
{
    public class SortCriteria
    {
        public string SortParameter { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SortDirection Direction { get; set; }
        public SortCriteria(string sortParameter, SortDirection direction)
        {
            this.SortParameter = sortParameter;
            this.Direction = direction;
        }
    }
}
