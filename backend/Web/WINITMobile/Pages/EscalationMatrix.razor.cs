using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WINITMobile.Services;
using WINITMobile.Models;
using Winit.Shared.Models.Common;
using Winit.Modules.Base.BL;
using WINITMobile.Pages.Base;
using Winit.Modules.Emp.Model.Classes;

namespace WINITMobile.Pages
{
    public partial class EscalationMatrix
    {
        [Inject] protected ApiService _apiService { get; set; }

        protected List<EscalationMatrixDto> matrixList;
        protected bool isLoading = true;
        protected string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                isLoading = true;


                var jobPositionUid = _appUser.SelectedJobPosition.UID;
                string endpoint = $"Emp/GetEscalationMatrix?jobPositionUid={jobPositionUid}";
                var apiResponse = await _apiService.FetchDataAsync<List<EscalationMatrixDto>>(
                    $"{_appConfigs.ApiBaseUrl}{endpoint}",
                    HttpMethod.Get
                );
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.Data != null)
                {
                    matrixList = apiResponse.Data;
                }
                else
                {
                    errorMessage = apiResponse?.ErrorMessage ?? "Failed to load escalation matrix.";
                }
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}