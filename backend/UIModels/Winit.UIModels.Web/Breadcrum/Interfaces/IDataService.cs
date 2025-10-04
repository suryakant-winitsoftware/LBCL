using Winit.UIModels.Web.Breadcrum.Interfaces;

namespace Winit.UIModels.Web.Breadcrum.Interfaces
{
    public interface IDataService
    {
        public string HeaderText { get; set; }
        public List<IBreadCrum> BreadcrumList { get; set; }
    }
}
