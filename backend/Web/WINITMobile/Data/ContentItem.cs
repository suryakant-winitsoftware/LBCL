using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Data;
public class ContentItem
{
    public string Text { get; set; }
    public bool IsChecked { get; set; }
    public bool Selected { get; set; }
    public ContentItem(string text)
    {
        Text = text;
    }
}

