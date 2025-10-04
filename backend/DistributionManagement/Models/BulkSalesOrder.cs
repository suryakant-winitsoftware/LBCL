using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionManagement.Models
{
    
        public class SKUCategory
        {
        public bool IsChecked { get; set; }
        public string Id { get; set; }
            public string Name { get; set; }
            public string ParentId { get; set; }
            public bool IsPromo { get; set; }
        public bool IsMCL { get; set; }

    }

       

        public class SelectCategoryType
        {
            public bool IsSelected = false;
            public string Id { get; set; }
            public string Name { get; set; }

        }


        public class SelectCategory
        {
            public bool IsSelected = false;
            public string Id { get; set; }
            public string Name { get; set; }
            public string ParentId { get; set; }
        }


       
    
}
