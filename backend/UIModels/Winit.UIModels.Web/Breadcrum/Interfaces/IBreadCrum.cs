namespace Winit.UIModels.Web.Breadcrum.Interfaces
{
    public interface IBreadCrum
    {
        public int SlNo { get; set; }
        public string Text { get; set; }
        public bool IsClickable { get; set; }
        public string URL { get; set; }
    }
}
