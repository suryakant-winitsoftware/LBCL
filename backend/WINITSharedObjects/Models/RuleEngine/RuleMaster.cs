using System;
using System.Collections.Generic;
using System.Text;

namespace WINITSharedObjects.Models.RuleEngine
{
    public class RuleMaster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
