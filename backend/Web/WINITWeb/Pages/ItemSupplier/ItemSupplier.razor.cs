using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Winit.Modules.SKU.Model.UIInterfaces;
using Winit.Shared.Models.Enums;
using Winit.Modules.ReturnOrder.Model.Interfaces;
using Winit.Modules.ReturnOrder.Model.Classes;
using System.Net.Http;
using System.Text;
using Winit.Modules.Common.Model.Interfaces;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Classes;
using System.ComponentModel.DataAnnotations;
using Winit.Shared.Models.Common;
using Winit.Modules.SKU.Model.Interfaces;
using Newtonsoft.Json.Linq;
using Winit.Modules.Common.BL;

using Winit.Modules.ReturnOrder.BL.Classes;
using Winit.Shared.Models.Enums;
using System.Reflection;
using WinIt.Pages.Base;
using Winit.Modules.Org.Model.Classes;
using Winit.Modules.Org.Model.Interfaces;
using Nest;
using NPOI.SS.Formula.Functions;
using System.Globalization;
using System.Resources;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.ItemSupplier
{
    public partial class ItemSupplier:BaseComponentBase
    {                 
        [Parameter]
        public EventCallback<string> OnDeleteItem { get; set; }
        [Parameter]
        public List<IOrg> DisplayedORGList { get; set; } = new List<IOrg>();
        private bool showFilter = false;
       
        public List<DataGridColumn> DataGridColumns { get; set; }
        private bool IsDeleteBtnPopUp { get; set; }
        private bool IsInitialized { get; set; }
        public IOrg? SelectedOrg { get; set; }
       
        public string selectedName { get; set; }
        public string CodeFilter { get; set; }
        public string NameFilter { get; set; }
       
      
        private void ToggleFilter()
        {
            showFilter = !showFilter;
        }
        protected override async Task OnInitializedAsync()
        {
            LoadResources(null, _languageService.SelectedCulture);
            await GenerateGridColumns();
            await _orgViewModel.PopulateViewModel();
            IsInitialized = true;
        }
       
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn { Header = "Supplier Code", GetValue = s => ((IOrg)s)?.Code ?? "N/A" },
                new DataGridColumn { Header = "Supplier Description", GetValue = s => ((IOrg)s)?.Name?? "N/A" },


             new DataGridColumn
            {
                Header = "Actions",
                IsButtonColumn = true,
                ButtonActions = new List<ButtonAction>
                {
                    new ButtonAction
                    {
                        Text = @Localizer["edit"],
                        Action = item => OnEditClick((IOrg)item)

                    },
                      new ButtonAction
                      {
                        Text = @Localizer["delete"],
                        Action = s => OnDeleteClick((IOrg)s)
                      }

                }
            }
             };
        }
        public void OnDeleteClick(IOrg orgItem)
        {
            SelectedOrg = orgItem;
            IsDeleteBtnPopUp = true;
            StateHasChanged();
        }
        public void OnEditClick(IOrg org)
        {
            _navigationManager.NavigateTo($"AddItemSupplier?OrgUID={org.UID}");
        }


        public async Task OnOkFromDeleteBTnPopUpClick()
        {
            IsDeleteBtnPopUp = false;
            string s = await _orgViewModel.DeleteItem(SelectedOrg?.UID);
            if (s.Contains("Failed"))
            {
                await _AlertMessgae.ShowErrorAlert(@Localizer["failed"], s);
            }
            else
            {
                await _AlertMessgae.ShowSuccessAlert(@Localizer["success"], s);
                await _orgViewModel.PopulateViewModel();

            }
            StateHasChanged();
        }
        public async void ApplyFilter()
        {
            List<FilterCriteria> filterCriterias = new List<FilterCriteria>();
            
            if (CodeFilter!=null)filterCriterias.Add(new FilterCriteria("Code", CodeFilter, FilterType.Like));
            if(NameFilter!=null)filterCriterias.Add(new FilterCriteria("Name", NameFilter, FilterType.Like));
            
            await _orgViewModel.ApplyFilter(filterCriterias);
            StateHasChanged();
        }
        public async void ResetFilter()
        {
            // Clear the filter criteria
            CodeFilter = null;
            NameFilter = null;
            await _orgViewModel.ResetFilter();
            StateHasChanged();
        }
      

        private async Task AddNewItem()
        {
            _navigationManager.NavigateTo($"AddItemSupplier");
        }

    }
}
