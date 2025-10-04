using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using Winit.Modules.Common.UIState.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using WinIt.BreadCrum.Classes;
using WinIt.Pages.Base;


namespace WinIt.Pages.SalesManagement.Distributor
{
    public partial class MaintainDistributor
    {
        protected override void OnInitialized()
        {
            BreadCrumDTO.HeaderText = "Maintain Distributor";
            BreadCrumDTO.BreadcrumList = [new Winit.UIModels.Web.Breadcrum.Classes.BreadCrumModel(1, "Maintain Distributor")];
            LoadResources(null, _languageService.SelectedCulture);
            SetColumnHeaders();
            FilterInitialized();

        }
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            await _maintainDistributor.PopulateViewModel();
            await StateChageHandler();
            _maintainDistributor.IsLoad = true;
            await base.OnInitializedAsync();
            HideLoader();
        }
        public List<FilterModel> ColumnsForFilter = [];



        public void FilterInitialized()
        {

            ColumnsForFilter = new List<FilterModel>
             {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox,ColumnName = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.Code), Label = @Localizer["distributor_code"]},
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.Name), Label = @Localizer["distributor_name"]},
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.SequenceCode), Label =@Localizer["prefix"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, ColumnName =nameof(Winit.Modules.Distributor.Model.Classes.Distributor.ContactNumber) , Label =@Localizer["contact_number"] },
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown,ColumnName=nameof(Winit.Modules.Distributor.Model.Classes.Distributor.Status),Label =@Localizer["status"] ,
                                   DropDownValues= new() {
                                                            new SelectionItem() { UID="1",Label="Active",Code="Active"},
                                                            new SelectionItem() { UID = "0", Label = "In Active", Code = "InActive" },
                                                            new SelectionItem() { UID="-1",Label="Blocked",Code="Blocked"}
                                                         }
                 },
             };
        }



        List<DataGridColumn> Columns { get; set; }
        protected void SetColumnHeaders()
        {
            Columns = new List<DataGridColumn>
            {
                 new DataGridColumn { Header = @Localizer["code"], GetValue = s => ((Winit.Modules.Distributor.Model.Classes.Distributor)s).Code, IsSortable = true, SortField = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.Code) },
                 new DataGridColumn { Header = @Localizer["name"], GetValue = s => ((Winit.Modules.Distributor.Model.Classes.Distributor)s).Name, IsSortable = true, SortField = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.Name) },
                 new DataGridColumn { Header = @Localizer["open_account_date"], GetValue = s => Winit.Shared.CommonUtilities.Common.CommonFunctions.GetDateTimeInFormat(((Winit.Modules.Distributor.Model.Classes.Distributor)s).OpenAccountDate),
                                                    IsSortable = true, SortField = "OpenAccountDate" },
                 new DataGridColumn { Header = @Localizer["prefix"], GetValue = s => ((Winit.Modules.Distributor.Model.Classes.Distributor)s).SequenceCode, IsSortable = false, SortField = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.SequenceCode) },
                 new DataGridColumn { Header = @Localizer["contact_person"], GetValue = s => ((Winit.Modules.Distributor.Model.Classes.Distributor)s).ContactPerson, IsSortable = false, SortField = nameof(Winit.Modules.Distributor.Model.Classes.Distributor.ContactPerson) },
                 new DataGridColumn { Header = @Localizer["contact_number"], GetValue = s => ((Winit.Modules.Distributor.Model.Classes.Distributor)s).ContactNumber },
                 new DataGridColumn { Header = @Localizer["status"], GetValue = s => (( Winit.Modules.Distributor.Model.Classes.Distributor)s).Status },

                 new DataGridColumn
                 {
                     Header = @Localizer["actions"],
                     IsButtonColumn = true,
                     ButtonActions = new List<ButtonAction>
                     {
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Text,
                             Text=@Localizer["distributor_admin"],
                             Action = item=>
                             {
                                 if(item is Winit.Modules.Distributor.Model.Classes.Distributor dist)
                                     _navigationManager.NavigateTo($"distributoradmin?OrgUID={dist.UID}&Status={dist.Status}");
                             }
                         },
                         new ButtonAction
                         {
                             ButtonType=ButtonTypes.Text,
                             Text = @Localizer["details"],
                             Action = item =>
                             {
                                  if(item is Winit.Modules.Distributor.Model.Classes.Distributor dist)
                                             _navigationManager.NavigateTo($"newdistributor?UID={dist.UID}&{PageType.Page}={PageType.Edit}");
                              }

                         },

                     }
                 }
           };
        }
        private async Task StateChageHandler()
        {
            ShowLoader();
            _navigationManager.LocationChanged += (sender, args) => SavePageState();
            bool stateRestored = _pageStateHandler.RestoreState("maintaindistributor", ref ColumnsForFilter, out PageState pageState);
            await OnFilterApply(_pageStateHandler._currentFilters);
            HideLoader();
        }
        private void SavePageState()
        {
            _navigationManager.LocationChanged -= (sender, args) => SavePageState();
            _pageStateHandler.SaveCurrentState("maintaindistributor");
        }
        private async Task OnFilterApply(Dictionary<string, string> filter)
        {
            ShowLoader();
            _pageStateHandler._currentFilters = filter;
            await _maintainDistributor.OnFilterApply(filter);
            HideLoader();
        }

    }
}
