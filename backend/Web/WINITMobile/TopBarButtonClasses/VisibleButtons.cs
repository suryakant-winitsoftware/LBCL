using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.TopBarButtonClasses
{
    public class VisibleIsButtonVisibles
    {
        public bool IsButtonVisible1 { get; set; }
        public bool IsButtonVisible2 { get; set; }
        public bool IsButtonVisible3 { get; set; }
        public VisibleIsButtonVisibles(bool IsButtonVisible1, bool IsButtonVisible2, bool IsButtonVisible3)
        {
            this.IsButtonVisible1 = IsButtonVisible1;
            this.IsButtonVisible3 = IsButtonVisible3;
            this.IsButtonVisible2 = IsButtonVisible2;
        }
    }
}
