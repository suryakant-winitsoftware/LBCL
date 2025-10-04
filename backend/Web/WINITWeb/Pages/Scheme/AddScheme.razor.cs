

using Microsoft.AspNetCore.Components;

namespace WinIt.Pages.Scheme
{
    public partial class AddScheme
    {
        Winit.UIModels.Web.Breadcrum.Interfaces.IDataService dataService = new Winit.UIModels.Web.Breadcrum.Classes.DataServiceModel()
        {
            HeaderText = "Manage Scheme",
            BreadcrumList = new List<Winit.UIModels.Web.Breadcrum.Interfaces.IBreadCrum>()
            {
                 new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Manage Scheme",URL="ManageScheme",IsClickable=true},
                 new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(){SlNo=1,Text="Add Scheme" },
             }
        };

        string scheme = "SellInSchemeBranchView";
        protected void OnSelectionChange(ChangeEventArgs e)
        {
            scheme = e.Value.ToString();
        }



    }
}
