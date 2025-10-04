using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WINITMobile.Data;
namespace WINITMobile.Data
{
    public class TabData
    {
        public string TabText { get; set; }
        public List<Winit.Shared.Models.Common.SelectionItem> ContentList { get; set; }

        public TabData(string tabText, List<Winit.Shared.Models.Common.SelectionItem> contentList)
        {
            TabText = tabText;
            ContentList = contentList;
        }

    }
}
