using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Constants;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WinIt.Pages.Customer_Details
{
    public partial class AsmMapping : ComponentBase
    {
        [Parameter] public EventCallback<List<IAsmDivisionMapping>> SaveOrUpdateAsmDivision { get; set; }
        [Parameter] public List<ISelectionItem> ASMselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> AsmItems { get; set; }
        [Parameter] public List<ISelectionItem> ASEMselectionItems { get; set; }
        [Parameter] public List<ISelectionItem> DivisionselectionItems { get; set; }
        [Parameter] public EventCallback<IAsmDivisionMapping> DeleteAsmDivision { get; set; }
        [Parameter] public string StoreUID { get; set; }
        [Parameter] public List<IAsmDivisionMapping> AsmDivisionDetails { get; set; } = new List<IAsmDivisionMapping>();
        public List<DataGridColumn> DataGridColumnsForAsm { get; set; }
        public List<DataGridColumn> DataGridColumnsForAsmPopUp { get; set; }
        public List<IAsmDivisionMapping> AsmDivisionDetailsDB { get; set; } = new List<IAsmDivisionMapping>();
        IAsmDivisionMapping obj { get; set; } = new AsmDivisionMapping();
        private bool IsAsmAdd { get; set; } = false;
        [Parameter] public bool IsAsmMappingSuccess { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GenerateAsmGridColumns();
            StateHasChanged();
        }
        protected override async Task OnParametersSetAsync()
        {
            try
            {
                if (_iAppUser.Role.Code == StoreConstants.ASEM)
                {
                    AsmItems = ASEMselectionItems;
                }
                else
                {
                    AsmItems = ASMselectionItems;
                }
            }
            catch (Exception)
            {

                throw;
            }
            StateHasChanged();
        }
        private async Task GenerateAsmGridColumns()
        {
            DataGridColumnsForAsm = new List<DataGridColumn>
            {

               // new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Division", GetValue = s => ((IAsmDivisionMapping)s)?.DivisionName ?? "N/A"},
                new DataGridColumn {Header = "ASM", GetValue = s => ((IAsmDivisionMapping)s)?.AsmEmpName ?? "N/A"},
                new DataGridColumn
                {
                    Header = "Actions",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/delete.png",
                            Action = item => OnDeleteClickAsm((IAsmDivisionMapping)item)

                        }
                    }
                }
            };
            DataGridColumnsForAsmPopUp = new List<DataGridColumn>
            {

               // new DataGridColumn {Header = "Address Person Name", GetValue = s => ((IAddress)s)?.Name ?? "N/A" },
                new DataGridColumn {Header = "Division", GetValue = s => ((IAsmDivisionMapping)s)?.DivisionName ?? "N/A"},
                new DataGridColumn {Header = "ASM", GetValue = s => ((IAsmDivisionMapping)s)?.AsmEmpName ?? "N/A"},
            };
        }
        public async Task OnDeleteClickAsm(IAsmDivisionMapping asmDivision)
        {
            bool AsmDelete = await _alertService.ShowConfirmationReturnType("Alert", "Are you sure you want to Delete this item?", "Yes", "No");
            if (AsmDelete)
            {
                AsmDivisionDetails.Remove(asmDivision);
                DeleteAsmDivision.InvokeAsync(asmDivision);
            }
            StateHasChanged();
        }
        private async Task OpenAsmPopUp()
        {
            IsAsmAdd = !IsAsmAdd;
        }
        public async Task ASMSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                obj.AsmEmpUID = selecetedValue?.UID;
                obj.AsmEmpName = selecetedValue?.Label;
                StateHasChanged();
            }
            else
            {
                obj.AsmEmpUID = string.Empty;
                obj.AsmEmpName = string.Empty;
            }
        }
        public async Task DivisionSelection(Winit.Shared.Models.Events.DropDownEvent dropDownEvent)
        {
            //SelectedAddress.Line4 = null;
            if (dropDownEvent.SelectionItems != null && dropDownEvent.SelectionItems.Any())
            {
                var selecetedValue = dropDownEvent.SelectionItems.FirstOrDefault();
                obj.DivisionUID = selecetedValue?.UID;
                obj.DivisionName = selecetedValue?.Label;
                StateHasChanged();
            }
            else
            {
                obj.DivisionUID = string.Empty;
                obj.DivisionName = string.Empty;
            }
        }
        private async void AddAsmDivisionDetails()
        {
            if (string.IsNullOrEmpty(obj.DivisionUID) || string.IsNullOrEmpty(obj.AsmEmpUID))
                return;
            if (!AsmDivisionDetails.Any(p => p.DivisionUID.Equals(obj.DivisionUID) && p.AsmEmpUID.Equals(obj.AsmEmpUID)))
            {
                IAsmDivisionMapping asmDetails = new AsmDivisionMapping();
                asmDetails = obj.DeepCopy();
                AsmDivisionDetails.Add(asmDetails);
                //await SaveOrUpdateAsmDivision.InvokeAsync(AsmDivisionDetails);
            }
            StateHasChanged();
        }
        public async Task SaveOrUpdateAsmMapping()
        {
            try
            {
                if (AsmDivisionDetails.Any())
                {
                    AsmDivisionDetails.ForEach(p =>
                    {
                        if (string.IsNullOrEmpty(p.LinkedItemUID))
                        {
                            p.LinkedItemType = "Store";
                            p.LinkedItemUID = StoreUID;
                        }
                    });
                    AsmDivisionDetailsDB = AsmDivisionDetails;
                    _loadingService.ShowLoading();
                    await SaveOrUpdateAsmDivision.InvokeAsync(AsmDivisionDetailsDB);
                    if (IsAsmMappingSuccess)
                        _tost.Add("", "ASM mapped for Divisions Successfully", Winit.UIComponents.SnackBar.Enum.Severity.Success);
                    else
                        _tost.Add("", "ASM mapping Failed...", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
                else
                {
                    _tost.Add("", "Please add Division and ASM", Winit.UIComponents.SnackBar.Enum.Severity.Error);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _loadingService.HideLoading();
                StateHasChanged();
            }
        }
    }
}
