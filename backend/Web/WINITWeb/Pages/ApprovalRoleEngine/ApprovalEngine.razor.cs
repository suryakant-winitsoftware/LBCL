using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Components;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.UIModels.Common;
namespace WinIt.Pages.ApprovalRoleEngine
{
    public partial class ApprovalEngine
    {
        private string statusMessage = "Waiting for approval...";

        #region Parameter Initialized
        public int CurrentApprovalLevel { get; set; }
        public List<ApprovalLogDetail> ApprovalData { get; set; }

        public bool IsLogViewing = false;
        [Parameter]
        public bool ShowApprovalEngine { get; set; } = false;
        [Parameter]
        public Dictionary<string, List<EmployeeDetail>>? ApprovalUsercodes { get; set; } = new Dictionary<string, List<EmployeeDetail>>();
        public List<string> ApprovalRoles = new List<string>();
        [Parameter] public bool IsRejectButtonNeeded { get; set; } = false;
        [Parameter] public bool IsReassignButtonNeeded { get; set; } = false;
        public bool IsFirstExecute = true;
        public string? ApiUrl { get; set; }
        [Parameter] public string? RequestId { get; set; }
        [Parameter] public int RuleId { get; set; }
        [Parameter]
        public EventCallback<List<ApprovalStatusResponse>> OnApprovalTracker { get; set; }
        [Parameter]
        public EventCallback OnAfterApproveOrRejct { get; set; }
        [Parameter] public Func<ApprovalStatusUpdate, Task<ApprovalActionResponse>>? HandleApprovalAction { get; set; }
        private bool Loading { get; set; } = true;
        private bool Error { get; set; } = false;
        private string? ErrorMessage { get; set; }
        public string PendingLevel { get; set; }

        public bool IsReassignedDone = false;
        private bool IsModalVisible;
        private string? SelectedOption;
        private string? Remark;
        public string EmptyVarMessage { get; set; }
        private string Popuptype;

        private string ApproverType;
        private int ApproverLevel;
        private bool IsEmptyVar = false;
        private bool IsConfirmationVisible = false;
        private string ActionToConfirm = "";
        public bool IsFinalApproved { get; set; } = false;
        public int RejectionLevel { get; set; }
        public int CurrentUserLevel { get; set; }
        List<EmployeeDetail> ClickedRoleIds = new List<EmployeeDetail>();
        public bool AuthorizeApproverDetailClicked = false;
        #endregion

        #region InitialRender Async
        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine(ApiUrl);
            CurrentUserLevel = _approvalEngine.UserLevelRoleMap.FirstOrDefault(v => v.Value.StartsWith($"[{_iAppUser.Role.Code}]")).Key;
        }
        #endregion

        #region Popup Logic
        private void OpenConfirmation(string action)
        {
            if (!string.IsNullOrWhiteSpace(Remark))
            {
                if (action == ApprovalConst.Approved)
                {

                    ActionToConfirm = action;
                    IsConfirmationVisible = true;

                }
                else if (action == ApprovalConst.Rejected)
                {
                    ActionToConfirm = action;
                    IsConfirmationVisible = true;
                }
                else
                {
                    if (SelectedOption == null)
                    {
                        EmptyVarMessage = "Please select the level";
                        IsEmptyVar = true;
                    }
                    else
                    {
                        ActionToConfirm = action;
                        IsConfirmationVisible = true;
                    }
                }
            }
            else
            {
                EmptyVarMessage = "Please provide the remarks first.";
                IsEmptyVar = true;

            }
        }

        private async Task ConfirmAction()
        {
            IsConfirmationVisible = false;
            //if (actionToConfirm == ApprovalConst.Approved)
            //{
            //    await OnApprove_RejectHandle(actionToConfirm);
            //}
            //else 
            if (ActionToConfirm == ApprovalConst.Reassign)
            {
                await Reassign();
            }
            else
            {
                await OnApproveAndRejectHandle(ActionToConfirm);
            }
        }

        public void ShowRejectPopup(string actionType, int approvalLevelitem)
        {
            RejectionLevel = approvalLevelitem;
            Popuptype = actionType;
            IsModalVisible = true;
        }
        private void ShowPopup(string actionType)
        {
            if (actionType == "Approve")
            {
                Remark = ApprovalConst.Approved;
            }
            Popuptype = actionType;
            IsModalVisible = true;
        }
        private void ShowReassignPopup(string actionType, string appType, int level)
        {
            ApproverType = appType;
            ApproverLevel = level;
            Popuptype = actionType;
            IsModalVisible = true;
            _approvalEngine.ReAssignOptions = _approvalEngine.ReAssignOptions.Where(i => i < ApproverLevel).ToList();
        }
        private void ClosePopup()
        {
            IsModalVisible = false;
        }
        public void CloseConfirmation()
        {
            IsConfirmationVisible = false;
        }
        private void CloseEmptyValidationModal()
        {
            IsEmptyVar = false;
            EmptyVarMessage = null;
        }
        #endregion


        #region AfterParameter Set
        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(RequestId) && RequestId != "0" && _approvalEngine.ApprovalHierarchyData == null && ShowApprovalEngine)
            {
                _loadingService.ShowLoading();
                foreach (var usercodeList in ApprovalUsercodes)
                {
                    ApprovalRoles.AddRange(usercodeList.Value.Select(user => user.EmpCode));
                    _approvalEngine.ApprovalRoleCodes.Add(usercodeList.Key);
                }
                await FetchApprovalHierarchyStatus();
                CurrentUserLevel = _approvalEngine.UserLevelRoleMap.FirstOrDefault(v => v.Value.StartsWith($"[{_iAppUser.Role.Code}]")).Key;
                _loadingService.HideLoading();
            }
        }
        #endregion

        #region FetchDataMethods
        private async Task FetchApprovalHierarchyStatus()
        {
            Loading = true;
            Error = false;
            ErrorMessage = string.Empty;
            try
            {

                await _approvalEngine.FetchApprovalHierarchyStatus(RequestId);

            }
            catch (Exception ex)
            {
                Error = true;
                ErrorMessage = ex.Message;
                await _AlertMessgae.ShowErrorAlert("Errror", ex.Message);

            }
            finally
            {
                Loading = false;
            }
        }
        #endregion


        #region ApproveRejectReassignHandleLogic
        private async Task OnApproveAndRejectHandle(string actionType)
        {

            try
            {
                ClosePopup();
                _loadingService.ShowLoading();
                var payload = new ApprovalStatusUpdate
                {
                    Status = actionType,
                    Remarks = Remark!,
                    RequesterId = _iAppUser.Emp.Code,
                    RoleCode = _iAppUser.Role.Code,
                    RequestId = RequestId!,
                };
                if (actionType == ApprovalConst.Approved)
                {
                    payload.IsFinalApproval=IsFinalApproval();
                }
                Remark=null;
                ApprovalActionResponse approvalActionResponse = await HandleApprovalAction.Invoke(payload);
                if (!approvalActionResponse.IsApprovalActionRequired)
                {
                    if (await _approvalEngine.UpdateApprovalStatus(payload))
                    {
                        PendingLevel=null;
                        ShowSuccessSnackBar("Success", GetApproveRejectMesage(actionType, true));
                        await FetchApprovalHierarchyStatus();
                    }


                }
                else if (approvalActionResponse.IsSuccess)
                {
                    ShowSuccessSnackBar("Success", GetApproveRejectMesage(actionType, true));
                    PendingLevel=null;
                    await FetchApprovalHierarchyStatus();
                    await OnAfterApproveOrRejct.InvokeAsync();

                }
                else
                {
                    ShowSuccessSnackBar("Failed", GetApproveRejectMesage(actionType, false));
                }

            }
            catch (Exception ex)
            {
                await _AlertMessgae.ShowErrorAlert("Errror", ex.Message);
            }
            finally
            {
                _loadingService.HideLoading();
            }
        }
        public string GetApproveRejectMesage(string actionType, bool responseType)
        {
            if (actionType==ApprovalConst.Approved && responseType)
            {
                return "Approve successful";
            }
            else if (actionType==ApprovalConst.Approved && !responseType)
            {
                return "Failed to approve";
            }
            else if (actionType==ApprovalConst.Rejected && responseType)
            {
                return "Reject successful";
            }
            else
            {
                return "Failed to reject";
            }
        }
        public bool IsFinalApproval()
        {
            if (_approvalEngine?.ApprovalHierarchyData?.approvalStatus?.Count == 1)
            {
                return _approvalEngine.ApprovalHierarchyData.approvalStatus.First().Status == "Pending";
            }

            var approvedItems = _approvalEngine?.ApprovalHierarchyData?.approvalStatus?.Where(item => item.Status == "Approved").ToList();
            var pendingItems = _approvalEngine?.ApprovalHierarchyData?.approvalStatus?.Where(item => item.Status == "Pending").ToList();

            if (pendingItems?.Count != 1)
            {
                return false;// Return false if there isn't exactly one pending item
            }
            var maxApprovedLevel = approvedItems?.Max(item => item.ApproverLevel);
            var pendingLevel = pendingItems.First().ApproverLevel;
            return approvedItems?.Count == maxApprovedLevel;
        }
        private async Task Reassign()
        {

            try
            {
                if (!string.IsNullOrEmpty(SelectedOption))
                {
                    IsConfirmationVisible = false;
                    ClosePopup();
                    _loadingService.ShowLoading();

                    if (await _approvalEngine.Reassign(SelectedOption, Remark!, ApproverLevel, RequestId!, ApproverType))
                    {
                        PendingLevel=null;
                        IsReassignedDone = true;
                        ShowSuccessSnackBar("Success", "Reassign successful");
                    }
                    else
                    {
                        await _AlertMessgae.ShowErrorAlert("Errror", "Reassign failed");
                    }
                    Remark=null;
                }
                else
                    await _alertService.ShowErrorAlert("Choose Level", "Choose Level !");
            }
            catch (Exception ex)
            {
                await _AlertMessgae.ShowErrorAlert("Errror", ex.Message);
            }
            finally
            {
                _loadingService.HideLoading();
            }
        }

        #endregion


        #region Approval Delete Logic

        #endregion

        #region Approval log logic
        private void ShowLog(ApprovalStatusResponse approval)
        {
            CurrentApprovalLevel = approval.ApproverLevel;
            ApprovalData = _approvalEngine?.ApprovalHierarchyData?.approvalLog.Where(item => item.Level == approval.ApproverLevel || item.ReassignTo == approval.ApproverLevel.ToString()).ToList();
            IsLogViewing = true;
        }
        public void OnCloseLog(bool IsClose)
        {
            IsLogViewing = false;
        }
        #endregion
        public void ShowAuthorizeApprovers(ApprovalStatusResponse approval)
        {
            string ApproverId = approval.ApproverId;
            if (ApprovalUsercodes.ContainsKey(ApproverId))
            {
                ClickedRoleIds.AddRange(ApprovalUsercodes[ApproverId]);
            }
            AuthorizeApproverDetailClicked=true;

        }
        public void ChangeRuleID(int id)
        {
            RuleId = id;
        }
    }
}
