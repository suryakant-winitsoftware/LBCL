

namespace Winit.UIModels.Web.Breadcrum.Classes
{
    public class BreadCrumModel : Interfaces.IBreadCrum
    {
        public int SlNo { get; set; }
        public string Text { get; set; }
        public bool IsClickable { get; set; }
        public string URL { get; set; }
        public BreadCrumModel()
        {

        }

        public BreadCrumModel(int slNo, string text, bool isClickable = false, string uRL = null)
        {
            SlNo = slNo;
            Text = text;
            IsClickable = isClickable;
            URL = uRL;
        }
    }
}
