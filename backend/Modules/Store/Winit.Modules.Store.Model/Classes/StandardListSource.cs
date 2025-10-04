using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
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
