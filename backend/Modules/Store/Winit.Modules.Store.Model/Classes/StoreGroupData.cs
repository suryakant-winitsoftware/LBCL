using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreGroupData : BaseModelV2 ,IStoreGroupData
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
        public string PrimaryUID { get; set; }
        public string JsonData { get; set; }
        public string Label { get; set; }
        public string PrimaryType { get; set; }
        public string PrimaryLabel { get; set; }
        public bool IsSelected { get;set; }
    }
    public class StoreGroupDataFromJson
    {
        public string UID { get; set; }
        public string StoreGroupTypeName { get; set; }
        public int Level { get; set; }
        public string Label { get; set; }
    }

}
