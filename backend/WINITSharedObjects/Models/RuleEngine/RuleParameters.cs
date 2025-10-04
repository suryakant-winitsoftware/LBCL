using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class RuleParameter
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
    }

}
