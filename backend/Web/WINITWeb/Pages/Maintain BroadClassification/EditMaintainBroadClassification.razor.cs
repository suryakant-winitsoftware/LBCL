using Microsoft.AspNetCore.Components;
using Winit.Modules.BroadClassification.Model.Classes;
using Winit.Modules.BroadClassification.Model.Interfaces;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.Base;

namespace WinIt.Pages.Maintain_BroadClassification
{
    public partial class EditMaintainBroadClassification : BaseComponentBase
    {
        public string? BroadClassificationUID { get; set; }
        private bool IsInitialized { get; set; } = false;
        public bool ISShowUnAssignedCustomerClassificationForSelection { get; set; } = false;
        public string BroadClassificationName { get; set; }
        public bool IsActive { get; set; }
        private bool IsDeleteBtnPopUp { get; set; } = false;
        public List<IListItem> CustomerClassifications { get; set; }
        public List<IBroadClassificationLine> broadClassificationLines { get; set; }
        public List<DataGridColumn> DataGridColumns { get; set; }

        private bool _selectAllChecked = false;
        private IBroadClassificationLine _itemToDelete;
        private List<IBroadClassificationLine> _itemsToAdd = new List<IBroadClassificationLine>();
        private List<IBroadClassificationLine> _itemsToDelete = new List<IBroadClassificationLine>();
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Edit Broad Classification",
            BreadcrumList = new List<IBreadCrum>()
            {
               new BreadCrumModel(){SlNo=1,Text="Maintain Broad Classification",URL="MaintainBroadClassification",IsClickable=true },
               new BreadCrumModel(){SlNo=1,Text="Edit Broad Classification"},
            }
        };
        protected override async Task OnInitializedAsync()
        {
            ShowLoader();
            BroadClassificationUID = _commonFunctions.GetParameterValueFromURL("BCUID");
            LoadResources(null, _languageService.SelectedCulture);
            await _broadClassificationLineViewModel.PopulateViewModel();
            await _broadClassificationHeaderViewModel.PopulateViewModel();
            await _broadClassificationHeaderViewModel.PopulateBroadClassificationHeaderDetailsByUID(BroadClassificationUID);
            broadClassificationLines = _broadClassificationLineViewModel.broadClassificationLinelist;
            CustomerClassifications = _broadClassificationHeaderViewModel.ClassificationTypes;
            BroadClassificationName = _broadClassificationHeaderViewModel.viewBroadClassificationHeaderLineData?.Name ?? string.Empty;
            IsActive = _broadClassificationHeaderViewModel.viewBroadClassificationHeaderLineData?.IsActive ?? false;
            await GenerateGridColumns();
            IsInitialized = true;
            HideLoader();
        }
        private async Task GenerateGridColumns()
        {
            DataGridColumns = new List<DataGridColumn>
            {

                new DataGridColumn
                {
                    Header = "Customer Classification",
                    GetValue = item =>
                    {
                         var classificationLine = item as IBroadClassificationLine;
                            return classificationLine.ClassificationCode;
                    }
                },
                new DataGridColumn
                {
                    Header = "Action",
                    IsButtonColumn = true,
                    ButtonActions = new List<ButtonAction>
                    {
                        new ButtonAction
                        {
                            ButtonType = ButtonTypes.Image,
                            URL = "Images/delete.png",
                            Action = item => OnDeleteClick((IBroadClassificationLine)item )
                        }
                    }
                }
            };
        }
        public void ToggleAllCheckboxes()
        {
            _selectAllChecked = !_selectAllChecked;

            foreach (var classification in CustomerClassifications
                .Where(c => !broadClassificationLines.Any(line => line.ClassificationCode == c.Name)))
            {
                classification.IsSelected = _selectAllChecked;
            }
            StateHasChanged();
        }

        private void OnCheckboxChange(ChangeEventArgs e, IListItem classification)
        {
            classification.IsSelected = (bool)e.Value;

            _selectAllChecked = CustomerClassifications
                .Where(c => !broadClassificationLines.Any(line => line.ClassificationCode == c.Name))
                .All(c => c.IsSelected);

            StateHasChanged();
        }

        private void OnRowClicked(IListItem classification)
        {
            classification.IsSelected = !classification.IsSelected;
            _selectAllChecked = CustomerClassifications
                .Where(c => !broadClassificationLines.Any(line => line.ClassificationCode == c.Name))
                .All(c => c.IsSelected);
            StateHasChanged();
        }

        private async Task OnAddBroadCustomerClassification()
        {
            var selectedItems = CustomerClassifications
                .Where(c => c.IsSelected && !broadClassificationLines.Any(line => line.ClassificationCode == c.Name))
                .ToList();

            foreach (var selectedItem in selectedItems)
            {
                var newLine = new BroadClassificationLine
                {
                    ClassificationCode = selectedItem.Name,
                    BroadClassificationHeaderUID = BroadClassificationUID,
                };
                broadClassificationLines.Add(newLine);
                _itemsToAdd.Add(newLine); // Track the addition
            }

            ISShowUnAssignedCustomerClassificationForSelection = false;
            StateHasChanged();
        }
        private async Task OnDeleteClick(IBroadClassificationLine item)
        {
            _itemToDelete = item;
            IsDeleteBtnPopUp = true;
            StateHasChanged();
        }

        private async Task ShowUnAssignedCustomerClassificationForSelection()
        {
            ISShowUnAssignedCustomerClassificationForSelection = true;
        }
        public async Task OnOkFromDeleteBTnPopUpClick()
        {
            IsDeleteBtnPopUp = false;
            ShowLoader();
            if (_itemToDelete != null)
            {
                broadClassificationLines.Remove(_itemToDelete);
                _itemsToDelete.Add(_itemToDelete);
                await _broadClassificationLineViewModel.DeleteBroadClassificationLineData(_itemToDelete);
                _itemToDelete = null;
            }
            HideLoader();
            StateHasChanged();
        }
        public async Task UpdateBroadCustomerClassification()
        {
            ShowLoader();

            foreach (var newItem in _itemsToAdd)
            {
                newItem.BroadClassificationHeaderUID = BroadClassificationUID;
                await _broadClassificationLineViewModel.CreateUpdateBroadClassificationLineData(newItem, true);
            }
            foreach (var item in _itemsToDelete)
            {
                await _broadClassificationLineViewModel.DeleteBroadClassificationLineData(item);
            }
            int itemCount = broadClassificationLines.Count(p => p.BroadClassificationHeaderUID == BroadClassificationUID);
            var broadClassificationHeader = new BroadClassificationHeader
            {

                UID = BroadClassificationUID,
                Name = BroadClassificationName,
                IsActive = IsActive,
                ClassificationCount = itemCount,
            };
            bool result = await _broadClassificationHeaderViewModel.CreateUpdateBroadClassificationHeaderData(broadClassificationHeader, true);

            HideLoader();

            if (result)
            {
                _navigationManager.NavigateTo("MaintainBroadClassification");
            }
        }
        public void BackBtnClicked()
        {
            _navigationManager.NavigateTo($"MaintainBroadClassification");
        }
    }
}
