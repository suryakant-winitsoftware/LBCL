using Microsoft.AspNetCore.Components;
using Winit.UIComponents.Common;
using Winit.UIModels.Common;
using WinIt.Pages.ApprovalRoleEngine;
using WinIt.Pages.Base;

namespace WinIt.Pages.Scheme
{
    public class SchemeBaseComponent : BaseComponentBase
    {

        //------------------------------------Approval Engine Code------------------------

        //public string RoleCode { get; set; } = "ASEM";//RoleCode From Session
        //protected bool Loading { get; set; } = false;
        //protected bool Error { get; set; } = false;

        //protected string ApprovalEngineApiURL = "https://approvalengineapi-dev.winitsoftware.com/v1/api/ruleengine/";
        //public int RuleId { get; set; } = 11;
        protected ApprovalEngine? childComponentRef;
        [Inject] protected Winit.UIComponents.Common.Services.ILoadingService? _loadingService { get; set; }
        protected string? ErrorMessage { get; set; }
        protected int RequestId { get; set; }
        [Inject] protected IAlertService _alertService { get; set; }
        [Inject] protected IServiceProvider _serviceProvider { get; set; }
        protected async Task executeRule(string UserCode, string UserType, int RequestId = 0)
        {
            if (RequestId == 0)
            {
                _loadingService.ShowLoading();
                Dictionary<string, object> payload = new Dictionary<string, object>();//according to Rule
                payload.Add("RequesterId", UserCode);
                payload.Add("Remarks", "Need approval");
                payload.Add("Customer", new Winit.UIModels.Common.Customer { FirmType = UserType });
                if (childComponentRef != null)
                {
                    //await childComponentRef.ExecuteRule(payload);
                }
                else
                {
                    Console.WriteLine($"{childComponentRef} is null");
                }
                _loadingService.HideLoading();
            }
        }

        //protected async Task HandleError(string error)
        //{
        //    // Set the error message
        //    ErrorMessage = error;
        //    await _alertService.ShowErrorAlert("Error", ErrorMessage);
        //}
        //protected async virtual Task HandleUpdateRequest(ApprovalApiResponse<dynamic> res)
        //{
        //    ErrorMessage = res.Message;
        //    if (res.Success)
        //    {
        //        await _alertService.ShowSuccessAlert("Success", ErrorMessage);

        //    }
        //    else
        //    {
        //        await _alertService.ShowErrorAlert("Error", ErrorMessage);
        //    }
        //}
        //protected async virtual Task HandleExecuteRule(ApprovalApiResponse<ApprovalStatus> res)
        //{
        //    if (res.Success)
        //    {
        //        RequestId = (int)res.data.RequestId;
        //        //insert into AllApprovalRequest(linkedItemType,linkedItemUID,requestID)values('Store',StoreUID,RequestId.Tostring())
        //    }
        //    else
        //        await _alertService.ShowErrorAlert("Error", res.Message);
        //}


    }
}
