using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Location.Model.Interfaces;

namespace Winit.Modules.Location.Model.Classes
{
    public class LocationData:BaseModel, ILocationData
    {
        public string L1 { get; set; }
        public string L2 { get; set; }
        public string L3 { get; set; }
        public string L4 { get; set; }
        public string L5 { get; set; }
        public string L6 { get; set; }
        public string L7 { get; set; }
        public string L8 { get; set; }
        public string L9 { get; set; }
        public string L10 { get; set; }
        public string L11 { get; set; }
        public string L12 { get; set; }
        public string L13 { get; set; }
        public string L14 { get; set; }
        public string L15 { get; set; }
        public string L16 { get; set; }
        public string L17 { get; set; }
        public string L18 { get; set; }
        public string L19 { get; set; }
        public string L20 { get; set; }
        public string PrimaryUid { get; set; }
        public string PrimaryLabel { get; set; }
        public string JsonData { get; set; }
        public string Label { get; set; }
        public bool IsSelected {  get; set; }
    }
}
