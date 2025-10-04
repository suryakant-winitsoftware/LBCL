using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using NPOI.POIFS.NIO;
using NPOI.SS.Formula.Functions;
using Practice;
using System.Globalization;
using System.Linq;
using System.Resources;
using Winit.Modules.Common.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;


namespace WinIt.Pages.LoadTemplate
{
    public partial class ViewLoadTemplate
    {

     
        private readonly IAppUser _appUser;
        private Winit.UIComponents.Web.Filter.Filter filterRef;

        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        public string OrgUID = Winit.Modules.Base.Model.CommonConstant.ORGUID != null ? Winit.Modules.Base.Model.CommonConstant.ORGUID : "FR001";

        public async Task SetHeaderName()
        {
            //_IDataService.BreadcrumList = new();

            //_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_load_request_templates"], IsClickable = false });
            //_IDataService.HeaderText = @Localizer["maintain_load_request_templates"] ;
            //await CallbackService.InvokeAsync(_IDataService);
        }

        IDataService dataService = new DataServiceModel()
        {
            BreadcrumList = new List<IBreadCrum>()
            {
            }
        };

        protected override async Task OnInitializedAsync()
        {
            dataService.HeaderText=@Localizer["maintain_load_request_templates"];
            dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["maintain_load_request_templates"], IsClickable = false });
            LoadResources(null, _languageService.SelectedCulture);
			_loadingService.ShowLoading(@Localizer["loading_load_request"]);
            await Task.Run(async () =>
            {
                await SetViewState();
                InvokeAsync(() =>
                {
                    _loadingService.HideLoading();
                });


            });

        }
		
		public async Task SetViewState()
        {
            await _RouteLoadViewModel.PopulateViewModel(OrgUID);
            //await SetHeaderName();
            FilterInitialized();
        }


        private async Task DeleteRouteloadTemplate(IRouteLoadTruckTemplateUI row)
        {
            bool result = await _alertService.ShowConfirmationReturnType("", $"{@Localizer["are_you_sure_you_want_to_delete?"]}", @Localizer["yes"], @Localizer["no"]);
            if (result)
            {
                try
                {

                    string selectedUID = row.UID.ToString();
                    if (await _RouteLoadViewModel.DeleteRouteLoadTruckTemplate(selectedUID))
                    {
                        _tost.Add(@Localizer["template"], @Localizer["template_delete_successfully"] , Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        _loadingService.ShowLoading(@Localizer["loading_load_template"]);
                        await _RouteLoadViewModel.PopulateViewModel(OrgUID);
                        InvokeAsync(() =>
                        {
                            StateHasChanged();
                            _loadingService.HideLoading();

                            // Ensure UI reflects changes
                        });
                        //StateHasChanged();

                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                }
            }

        }
        private string GetRowClass(IRouteLoadTruckTemplateUI row)
        {
            return row.IsSelected ? "table-danger" : "";
        }

        private RouteLoadTruckTemplate selectedRow;
        private void EditRouteloadTemplate(IRouteLoadTruckTemplateUI row)
        {
            var RouteLoadTruckTemplateUID = row.UID;

            NavManager.NavigateTo($"addeditloadtemplate?UID={RouteLoadTruckTemplateUID}");

        }

        private async Task AddNewProduct()
        {

            NavManager.NavigateTo($"addeditloadtemplate");
        }

        //Check Null Logic
      


        //Filter Logic
        public List<FilterModel> ColumnsForFilter;
        private bool showFilterComponent = false;
        public async void ShowFilter()
        {
           
            filterRef.ToggleFilter();
        }

        public void FilterInitialized()
        {
                List<ISelectionItem> TempletateDDLValues = Winit.Shared.CommonUtilities.Common.CommonFunctions.ConvertToSelectionItems(_RouteLoadViewModel.RouteList, new List<string> { "UID", "UID", "Name" });

                ColumnsForFilter = new List<FilterModel>
            {
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["template_name"],ColumnName = LoadTemplateConst.TemplateName},
                 new FilterModel { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, DropDownValues=TempletateDDLValues, Label = @Localizer["route"],ColumnName = LoadTemplateConst.Route,SelectionMode=SelectionMode.Multiple},
            };
            showFilterComponent = true;

        }

        private async Task OnFilterApply(Dictionary<string, string> filterCriterias)
        {
            if (filterCriterias == null)
                return;

            List<FilterCriteria> criteriaList = filterCriterias
                .Where(pair => !string.IsNullOrEmpty(pair.Value)) // Exclude entries with empty values
                .Select(pair =>
                {
                     if (pair.Key == "TemplateName")
                     {
                         return new FilterCriteria(pair.Key, pair.Value, FilterType.Like);
                     }

                    var values = pair.Value.Split(',').Select(value => value.Trim()).ToArray();

                    return new FilterCriteria(pair.Key, values, FilterType.In);

                }).ToList();

            await _RouteLoadViewModel.ApplyFilter(criteriaList);

        }
    }
}