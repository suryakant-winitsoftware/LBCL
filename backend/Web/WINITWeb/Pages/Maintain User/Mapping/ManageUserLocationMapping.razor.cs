using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Maintain_User.Mapping
{
    public partial class ManageUserLocationMapping
    {
        List<Winit.Modules.User.Model.Classes.UserLocationMapping> userLocationMappings=new List<Winit.Modules.User.Model.Classes.UserLocationMapping>();

        List<DataGridColumn> Columns;

        protected override Task OnInitializedAsync()
        {
            SetColumnsHeaders();
            return base.OnInitializedAsync();
        }
        protected void SetColumnsHeaders()
        {
            Columns = new List<DataGridColumn>()
            {
                new(){Header="Mapping Code" ,GetValue=s=>((Winit.Modules.User.Model.Classes.UserLocationMapping)s).MappingCode},
                new(){Header="Mapping Name" ,GetValue=s=>((Winit.Modules.User.Model.Classes.UserLocationMapping)s).MappingName},
                new(){Header="Is Active" ,GetValue=s=>CommonFunctions.GetBooleanValueInYesOrNO(((Winit.Modules.User.Model.Classes.UserLocationMapping)s).IsActive)},
            };
        }

    }
}
