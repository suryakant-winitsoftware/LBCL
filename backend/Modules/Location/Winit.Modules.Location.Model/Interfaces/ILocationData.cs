using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationData:IBaseModel
    {
        string L1 { get; set; }
        string L2 { get; set; }
        string L3 { get; set; }
        string L4 { get; set; }
        string L5 { get; set; }
        string L6 { get; set; }
        string L7 { get; set; }
        string L8 { get; set; }
        string L9 { get; set; }
        string L10 { get; set; }
        string L11 { get; set; }
        string L12 { get; set; }
        string L13 { get; set; }
        string L14 { get; set; }
        string L15 { get; set; }
        string L16 { get; set; }
        string L17 { get; set; }
        string L18 { get; set; }
        string L19 { get; set; }
        string L20 { get; set; }
        string PrimaryUid { get; set; }
        string PrimaryLabel { get; set; }
        string JsonData { get; set; }
        string Label { get; set; }
        bool IsSelected { get; set; }
    }
}
