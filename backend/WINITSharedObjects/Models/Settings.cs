using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models
{
    public class Settings
    {

        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public bool IsEditable { get; set; }
        public int SS { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public DateTime ServerAddTime { get; set; }
        public DateTime ServerModifiedTime { get; set; }


    }





}
