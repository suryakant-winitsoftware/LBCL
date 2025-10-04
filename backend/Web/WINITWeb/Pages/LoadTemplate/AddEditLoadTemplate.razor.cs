using Microsoft.AspNetCore.Components;
using Nest;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.Route.Model.Classes;
using Winit.Modules.Route.Model.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Events;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

namespace WinIt.Pages.LoadTemplate
{
    public partial class AddEditLoadTemplate
    {            
        private bool CheckAllRows = false;

        public List<string> ItemsAddedNow = new List<string>();
        public List<string> AlreadyAddedItems = new List<string>();
        

        public RouteLoadTruckTemplateViewDTO DataForUpdateAdd = new RouteLoadTruckTemplateViewDTO();
       
        private Winit.UIComponents.Web.DialogBox.AddSKU AddSKURef;
        public bool IsAddSKUDialogOpen = false;
       
       public string OrgUID = Winit.Modules.Base.Model.CommonConstant.ORGUID !=null? Winit.Modules.Base.Model.CommonConstant.ORGUID:"FR001";

		IDataService dataService = new DataServiceModel()
		{
			BreadcrumList = new List<IBreadCrum>()
			{
				
			}
		};

		public async Task SetHeaderName()
		{
			//_IDataService.BreadcrumList = new();
			//_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_load_template"], IsClickable = true, URL = "loadrequestemplate" });

			//_IDataService.BreadcrumList.Add(new BreadCrum.Classes.BreadCrumModel() { SlNo = 2, Text = _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null ? @Localizer["edit_load_template"] : @Localizer["add_load_template"], IsClickable = false });
			//_IDataService.HeaderText = _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null ? @Localizer["edit_load_template"] : @Localizer["add_load_template"];

			//await CallbackService.InvokeAsync(_IDataService);
		}
		protected override async Task OnInitializedAsync()
        {
            dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 1, Text = @Localizer["manage_load_template"], IsClickable = true, URL = "loadrequestemplate" });
            dataService.HeaderText=_AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null ? @Localizer["edit_load_template"] : @Localizer["add_load_template"];
            dataService.BreadcrumList.Add(new BreadCrumModel() { SlNo = 2, Text = _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null ? @Localizer["edit_load_template"] : @Localizer["add_load_template"], IsClickable = false });
            LoadResources(null, _languageService.SelectedCulture);
            _loadingService.ShowLoading(@Localizer["loading_load_template"]);
            await Task.Run(async () =>
            {

                await SetAddEditState();
                InvokeAsync(async () =>
                {
                    _loadingService.HideLoading();                                   
                });


            });
        }
       
        [CascadingParameter]
        public EventCallback<BreadCrum.Interfaces.IDataService> CallbackService { get; set; }
        protected override void OnInitialized()
        {
            InitializeVariables();
            base.OnInitialized();
        }
        public async Task SetAddEditState()
        {
            try
            {
                await FetchQueryStrings();
                await setVariables();               
                await SetHeaderName();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{@Localizer["error_loading_data:"]} {ex.Message}");
            }
        }

        public async Task FetchQueryStrings()
        {


            var uri = new Uri(NavManager.Uri);
            var query = uri.Query;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID = queryParams?.Get("UID");
            }
            else
            {
                _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO = null;
                _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID = null;
            }
        }

        public async Task setVariables()
        {
            if (_AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null)
            {
                await _AddEditRouteLoadViewModel.PopulateViewModel();

            }
            else
            {
               
            }
            await _AddEditRouteLoadViewModel.GetSKUMasterData();
            await GetRoutes();
        }
        public void InitializeVariables()
        {
            try
            { 
           
            if (_AddEditRouteLoadViewModel.RouteListForSelection != null)
            {
                _AddEditRouteLoadViewModel.RouteListForSelection.ForEach(route => route.IsSelected = false);
            }
        }
            catch(Exception ex) { Console.WriteLine(ex.Message.ToString()); }
        }

        
        public async Task GetRoutes()
        {
            if (_AddEditRouteLoadViewModel.RouteListForSelection == null || _AddEditRouteLoadViewModel.RouteListForSelection.Count <= 0)
            {
                await _AddEditRouteLoadViewModel.GetRoutes();
            }
            

        }
       



       


        public async Task ApplySearch(string searchString)
        {
            try
            {

               await _AddEditRouteLoadViewModel.ApplySearch(searchString);

            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

        }


        
        public async Task  OpenAddSKUDialog()
        {
            try { 
            _loadingService.ShowLoading(@Localizer["loading_sku's"]);
            await Task.Run(async () =>
            {
                await _AddEditRouteLoadViewModel.GetSKUMasterData();              
                IsAddSKUDialogOpen = true;
                AddSKURef?.OpenAddProductDialog();
                InvokeAsync(async () =>
                {
                    _loadingService.HideLoading();

                });


            });

            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public async Task SaveAndUpDateTemplates()
        {
            if (_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO == null || _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Count <= 0)
            {
                await _alertService.ShowErrorAlert("", @Localizer["add_atleast_one_sku"],null, @Localizer["ok"]);
            }
            else
            {
                
                if (await _alertService.ShowConfirmationReturnType("", @Localizer["do_you_want_to_save_changes?"], @Localizer["yes"], @Localizer["no"]))
                {
                    try
                    {
                        _loadingService.ShowLoading(@Localizer["template_data_updating"]);
                        await Task.Run(async () =>
                        {
                        
                                if (await _AddEditRouteLoadViewModel.CreateUpdateIRouteLoadTruckTemplateDTO())
                                {

                                    if (_AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null)
                                    {
                                        _tost.Add(@Localizer["template"], @Localizer["item_successfully_updated"] , Winit.UIComponents.SnackBar.Enum.Severity.Success);
                                    }
                                    else
                                    {
                                        _tost.Add(@Localizer["template"], @Localizer["item_successfully_added"], Winit.UIComponents.SnackBar.Enum.Severity.Success);

                                    }
                                }
                                else 
                                {
                                    if (_AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID != null)
                                    {
                                        _tost.Add (@Localizer["item"], @Localizer["template_updated_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);
                                    }
                                    else
                                    {
                                        _tost.Add(@Localizer["item"], @Localizer["template_add_failed"], Winit.UIComponents.SnackBar.Enum.Severity.Error);

                                    }
                                }
                            _AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID = _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate?.UID;

                            await _AddEditRouteLoadViewModel.PopulateViewModel();
                            InvokeAsync(() =>
                            {
                                _loadingService.HideLoading();
                            });

                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                if (_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO != null && _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList != null)
                {
                    foreach (var row in _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList)
                    {
                        row.ActionTypes = Winit.Shared.Models.Enums.ActionType.Update;
                    }


                }
            }
        }
        
        public void ResetVariables()
        {
            _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO = null;
            _AddEditRouteLoadViewModel.TemplateName = null;
            _AddEditRouteLoadViewModel.TemplateDescription = null;
            _AddEditRouteLoadViewModel.SelectedRouteUID = null;
            _AddEditRouteLoadViewModel.RouteListForSelection = null;
            
        }
        
       
        void UpdateProperty(IRouteLoadTruckTemplateLine row, ChangeEventArgs e, string propertyName)
        {
            var property = typeof(RouteLoadTruckTemplateLine).GetProperty(propertyName);

            if (property != null)
            {
                if (int.TryParse(e.Value.ToString(), out var newValue))
                {
                    property.SetValue(row, newValue);
                    row.ModifiedTime = DateTime.Now;
                    row.ServerModifiedTime = DateTime.Now;

                }
            }
        }

        bool isSkuPresent = false;
        public void GetSelectedSKU(List<ISelectionItem> selectionSKUs)
        {
            try
            {
               
                _AddEditRouteLoadViewModel.CreateInstancesOfTemplateDTO();
                if (_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Count == 0)
                {
                    _AddEditRouteLoadViewModel.LineNumber = 0;
                }
                else
                {
                    _AddEditRouteLoadViewModel.LineNumber = _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Max(item => item.LineNumber);
                }

                foreach (var selectionItem in selectionSKUs)
                {
                    if (_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Count>0)
                    {
                     isSkuPresent = _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Any(item => item.SKUCode == selectionItem.Code);
                    }
                    if (!isSkuPresent )
                    {
                        if (_AddEditRouteLoadViewModel.RouteLoadTruckTemplateUID == null &&  _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplate == null)
                        {

                            _AddEditRouteLoadViewModel.CreateRouteLoadTruckTemplate();
                        }
                        _AddEditRouteLoadViewModel.CreateRouteLoadTruckTemplateLine(selectionItem);
                        ItemsAddedNow.Add(selectionItem.Code);
                    }
                    else
                    {
                        AlreadyAddedItems.Add(selectionItem.Code);
                    }
                }
                if (ItemsAddedNow.Count > 0)
                {
                    // Construct the message based on the items added now
                    string message = string.Join(", ", ItemsAddedNow) + " ," + @Localizer["added_now"];

                    // Add the toast with the dynamic message
                    _tost.Add("Item", message, Winit.UIComponents.SnackBar.Enum.Severity.Normal);
                }
                if (AlreadyAddedItems.Count > 0)
                {
                    // Construct the message based on the items added now
                    string message = string.Join(", ", AlreadyAddedItems)+" ," + @Localizer["already_added"];

                    // Add the toast with the dynamic message
                    _tost.Add("Item", message, Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }

            }
            catch(Exception ex) { }
        }
        public async void OnselectRoue(DropDownEvent dropDowneven)
        {
            if(dropDowneven != null)
            { await _AddEditRouteLoadViewModel.OnselectRoue(dropDowneven); }

        }
                    
        private async Task<bool> DeleteSelectedTemplate()
        {
            try
            {

                if(_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO==null ||
                    _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList  == null 
                    || _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Count <=0
                    || !(_AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList.Any(line => line.IsSelected)))
                {
                    _tost.Add(@Localizer["alert"], @Localizer["please_select_atleast_one_line"] , Winit.UIComponents.SnackBar.Enum.Severity.Warning);

                }
               
                else
                {
                    _AddEditRouteLoadViewModel.MatchingNewExistingUIDs();
                    if (await _alertService.ShowConfirmationReturnType("", @Localizer["are_you_sure_you_want_to_delete_selected_item"] , @Localizer["yes"], @Localizer["no"]))
                    {
                        
                        if (_AddEditRouteLoadViewModel.ExistingUIDs == null || !_AddEditRouteLoadViewModel.ExistingUIDs.Any(existingUid => _AddEditRouteLoadViewModel.AllUIDs.Contains(existingUid)))
                        {
                            RemoveSelectedTemplateFromList();
                            _tost.Add(@Localizer["success"], @Localizer["template_successfully_deleted"], Winit.UIComponents.SnackBar.Enum.Severity.Success);
                        }
                        else
                        {

                            if (await _AddEditRouteLoadViewModel.DeleteSelectedTemplates())
                            {
                                _tost.Add(@Localizer["success"], @Localizer["template_successfully_deleted"] , Winit.UIComponents.SnackBar.Enum.Severity.Success);
                                _loadingService.ShowLoading(@Localizer["template_data_updating"]);
                                await Task.Run(async () =>
                                {

                                    await _AddEditRouteLoadViewModel.PopulateViewModel();
                                    InvokeAsync(() =>
                                    {
                                        _loadingService.HideLoading();
                                    });

                                });
                               
                            }
                            else
                            {
                                _tost.Add(@Localizer["failed"], @Localizer["template_deleted_failed"] , Winit.UIComponents.SnackBar.Enum.Severity.Error);
                            }


                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }


        public void RemoveSelectedTemplateFromList()
        {
            _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList = _AddEditRouteLoadViewModel.DisplayRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList
                              .Where(line => !line.IsSelected)
                              .ToList();
        }

        private void ToggleAllRows(ChangeEventArgs e)
        {
            CheckAllRows = !CheckAllRows;

            foreach (var row in _AddEditRouteLoadViewModel.FilterRouteLoadTruckTemplateViewDTO.RouteLoadTruckTemplateLineList)
            {
                row.IsSelected = CheckAllRows;
            }
        }

        private void ToggleRow(IRouteLoadTruckTemplateLine row)
        {
            row.IsSelected = !row.IsSelected;
        }
        private async Task BackBtnConfirmation()
        {
            ResetVariables();
            NavManager.NavigateTo("loadrequestemplate");
        }
       
    }
}
