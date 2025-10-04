using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Interfaces;

namespace Winit.Shared.Models.Classes
{
    public class StandardListSource : IStandardListSource
    {
        public string Id { get; set; }
        public string SourceUID { get; set; }
        public string SourceType { get; set; }
        public string SourceLabel { get; set; }
        public string SpinnerKeyName { get; set; }
        public int SerialNo { get; set; }
        public object obj { get; set; }
        public string BarCode { get; set; }
    }
}
