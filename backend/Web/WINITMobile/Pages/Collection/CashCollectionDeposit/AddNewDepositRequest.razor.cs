
using Newtonsoft.Json;
using Winit.Modules.Bank.Model.Interfaces;
using Winit.Modules.CollectionModule.BL.Classes.CreatePayment;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.FileSys.Model.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using WINITMobile.Data;
using WINITMobile.Pages.Base;

namespace WINITMobile.Pages.Collection.CashCollectionDeposit
{
    public partial class AddNewDepositRequest
    {
        public List<IBank> DropDown { get; set; } = new List<IBank>();
        public string ImgFolderPath { get; set; }
        public string RequestNo { get; set; } = "";
        private Dictionary<string, bool> checkboxStates { get; set; } = new Dictionary<string, bool>();
        public List<IFileSys> ImageFileSysList { get; set; }
        public List<IAccCollection> ReceiptRecords { get; set; } = new List<IAccCollection>();
        private List<IAccCollection> SelectedItems { get; set; } = new List<IAccCollection>();
        public bool IsView { get; set; } = false;
        public string SelectedTab { get; set; } = "";




        private FileCaptureData fileCaptureData = new FileCaptureData
        {
            AllowedExtensions = new List<string> { ".jpg", ".png" }, // Add allowed extensions
            IsCameraAllowed = true,
            IsGalleryAllowed = true,
            MaxNumberOfItems = 5,
            MaxFileSize = 10 * 1024 * 1024, // 10 MB
            EmbedLatLong = true,
            EmbedDateTime = true,
            LinkedItemType = "ItemType",
            LinkedItemUID = "ItemUID",
            EmpUID = "EmployeeUID",
            JobPositionUID = "JobPositionUID",
            IsEditable = true,
            Files = new List<FileSys>()
        };

        protected override async Task OnInitializedAsync()
        {
            await _collectionDepositViewModel.PopulateViewModel();
            ImgFolderPath = Path.Combine(_appConfigs.BaseFolderPath, FileSysTemplateControles.GetReturnOrderImageFolderPath("Request"));
            IsView = Convert.ToBoolean(GetParameterValueFromURL("IsView"));
            SelectedTab = GetParameterValueFromURL("Tab");
            _collectionDepositViewModel.CashCollectionDeposit = new AccCollectionDeposit();
            await _createPaymentAppViewModel.GetBank();
            DropDown = _createPaymentAppViewModel.Banks;
            if (!IsView)
            {
                _collectionDepositViewModel.CashCollectionDeposit.EmpUID = _appUser.Emp.UID;
                _collectionDepositViewModel.CashCollectionDeposit.JobPositionUID = _appUser.SelectedJobPosition.UID;
                _collectionDepositViewModel.CashCollectionDeposit.Status = ConstantVariables.Pending;
                _collectionDepositViewModel.CashCollectionDeposit.RequestDate = DateTime.Now;
                _collectionDepositViewModel.CashCollectionDeposit.RequestNo = _collectionDepositViewModel.RequestNumber;
                ReceiptRecords = await _collectionDepositViewModel.GetReceipts();
            }
            else
            {
                RequestNo = GetParameterValueFromURL("RequestNo");
                ReceiptRecords = await _collectionDepositViewModel.ViewReceipts(RequestNo);
            }
            StateHasChanged();
        }
        private string GetParameterValueFromURL(string paramName)
        {
            var uri = new Uri(_navigationManager.Uri);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);

            return queryParams.Get(paramName);
            //return "b5c6d66b-5c41-4d9b-9b0d-ddacc7697b89";
        }
        private async void OnDatePickerChanged(string newDate)
        {
            _collectionDepositViewModel.CashCollectionDeposit.RequestDate = Convert.ToDateTime(newDate);
        }
        private void OnImageDeleteClick(string fileName)
        {
            IFileSys fileSys = ImageFileSysList.Find
                (e => e.FileName == fileName);
            if (fileSys is not null) ImageFileSysList.Remove(fileSys);
        }
        private async Task OnImageCapture((string fileName, string folderPath) data)
        {
            IFileSys fileSys = ConvertFileSys("Request", _collectionDepositViewModel.RequestNumber, "Item", "Image",
                data.fileName, _appUser.Emp?.Name, data.folderPath);
            ImageFileSysList.Add(fileSys);
            await Task.CompletedTask;
        }
        public bool GetCheckboxState(string uid)
        {
            return checkboxStates.TryGetValue(uid, out bool state) ? state : false;
        }

        public void SetCheckboxState(string uid, bool state)
        {
            checkboxStates[uid] = state;
        }
        public async void CreateRequest()
        {
            try
            {
                if (await CheckValidations())
                {
                    _collectionDepositViewModel.CashCollectionDeposit.ReceiptNos = ConvertToJson();
                    bool requestResult = await _collectionDepositViewModel.CreateCashDepositRequest();
                    if (requestResult)
                    {
                        await _alertService.ShowSuccessAlert("Success", "Request created Successfully");
                        await SuccessNavigation();
                    }
                    else
                    {
                        await _alertService.ShowErrorAlert("Error", "Request failed!");
                    }
                }
                else
                {
                    //await _alertService.ShowErrorAlert("Error", "Please enter all fields.");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async void UpdateRequest()
        {
            try
            {
                string requestResult = await _collectionDepositViewModel.ApproveOrRejectDepositRequest(_collectionDepositViewModel.CashCollectionDeposit, "");
                if (requestResult == "1")
                {
                    await _alertService.ShowSuccessAlert("Success", "Request updated Successfully");
                    await SuccessNavigation();
                }
                else
                {
                    await _alertService.ShowErrorAlert("Error", "Request update failed!");
                }
            }
            catch(Exception ex)
            {

            }
        }
        public string ConvertToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(SelectedItems.Select(p => p.ReceiptNumber).ToList());
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        public async Task<bool> CheckValidations()
        {
            try
            {
                if (_collectionDepositViewModel.CashCollectionDeposit.Amount == 0 ||
                    string.IsNullOrEmpty(_collectionDepositViewModel.CashCollectionDeposit.BankUID) ||
                    string.IsNullOrEmpty(_collectionDepositViewModel.CashCollectionDeposit.Branch) ||
                    string.IsNullOrEmpty(_collectionDepositViewModel.CashCollectionDeposit.Notes) ||
                    SelectedItems.Count == 0)

                {
                    await _alertService.ShowErrorAlert("Error", "Please enter all fields.");
                    return false;
                }
                if (_collectionDepositViewModel.CashCollectionDeposit.Amount != SelectedItems.Where(p => p.IsExpanded).Sum(p => p.Amount))
                {
                    await _alertService.ShowErrorAlert("Error", "Amount mismatch");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new();
            }
        }
        public async Task SuccessNavigation()
        {
            try
            {
                _navigationManager.NavigateTo("maintaincashcollection");
            }
            catch (Exception ex)
            {

            }
        }
        private void ToggleSelection(IAccCollection item, string Name = null)
        {
            if (Name == "CheckAll")
            {
                bool checkAllState = !GetCheckboxState("CheckAll");
                SetCheckboxState("CheckAll", checkAllState);
                foreach (var payableItem in ReceiptRecords) // Assuming AllItems is a collection of all IAccPayable items
                {
                    SetCheckboxState(payableItem.UID, checkAllState);
                    if (checkAllState)
                    {
                        if (!SelectedItems.Contains(payableItem))
                        {
                            payableItem.IsExpanded = true;
                            SelectedItems.Add(payableItem);
                        }
                    }
                    else
                    {
                        payableItem.IsExpanded = false;
                        SelectedItems.Remove(payableItem);
                    }
                }
            }
            else
            {
                bool currentState = GetCheckboxState(item.UID);
                SetCheckboxState(item.UID, !currentState);
                if (SelectedItems.Contains(item))
                {
                    item.IsExpanded = false;
                    SelectedItems.Remove(item);
                }
                else
                {
                    item.IsExpanded = true;
                    SelectedItems.Add(item);
                }
            }
        }

        public decimal Amount()
        {
            try
            {
                return SelectedItems.Where(p => p.IsExpanded).Sum(p => p.Amount);

            }
            catch (Exception ex)
            {
                throw new();
            }
        }
    }
}
