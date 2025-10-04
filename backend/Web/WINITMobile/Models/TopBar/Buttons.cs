using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Models.TopBar
{
    public class Buttons
    {
        public string ButtonText { get; set; }
        public string URL { get; set; }
        public bool IsVisible { get; set; } = false;
        public ButtonType ButtonType { get; set; }

        public Action Action { get; set; }

    }
}
