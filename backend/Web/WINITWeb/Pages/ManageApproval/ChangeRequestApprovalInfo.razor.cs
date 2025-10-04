using System.ComponentModel.DataAnnotations.Schema;
using Winit.Modules.ApprovalEngine.Model.Classes;
using Winit.Modules.ApprovalEngine.Model.Constants;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Store.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.UIModels.Web.Breadcrum.Classes;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using WinIt.Pages.ApprovalRoleEngine;


namespace WinIt.Pages.ManageApproval
{
    public partial class ChangeRequestApprovalInfo
    {
        public bool IsPageLoading = true;
        public bool IsModalPopupTriggered = false;
        public string uid { get; set; } = default!;
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Change Request Approval Info",
            BreadcrumList = new List<IBreadCrum>()
            {
                new BreadCrumModel(){SlNo = 1, Text = "View All Request Change Approval", IsClickable = true, URL = "viewallchangerequestapprovalinfo"},
                new BreadCrumModel() {SlNo = 1, Text = "Change request approval info",IsClickable = false }
            }
        };
        protected override async Task OnInitializedAsync()
        {
            try
            {
                ShowLoader();
                FetchFromQuerySting();
                await _ApprovalEngine.GetChangeRequestDataByUIDAsync(uid);
                await _ApprovalEngine.GetRequestId(uid);
                //await OnAllApproved();
                HideLoader();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                IsPageLoading=false;
            }

        }
        public void FetchFromQuerySting()
        {
            var uri = new Uri(NavManager.Uri);
            var query = uri.Query;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                if (queryParams != null)
                {
                    uid = queryParams.Get("UID")!;
                }
            }
        }

        // Method to map the section name to the respective model type
        // Method to map the section name to the respective model type
        // Method to map the section name to the respective model type
        public Type GetModelType(string sectionName)
        {
            return sectionName switch
            {
                // Grouping cases for Store model
                OnboardingScreenConstant.CustomInfoStore => typeof(Winit.Modules.Store.Model.Classes.Store), // Store model class

                // Grouping cases for StoreAdditionalInfo model
                OnboardingScreenConstant.CustomInfoStoreAdditionInfo => typeof(StoreAdditionalInfo), // StoreAdditionalInfo model class

                // Grouping cases for StoreAdditionalInfoCMI model
                OnboardingScreenConstant.CustomInfoStoreAdditionInfoCmi or
                OnboardingScreenConstant.EmployeeDetails or
                OnboardingScreenConstant.ServiceCenterDetail or
                OnboardingScreenConstant.ShowroomDetails or
                OnboardingScreenConstant.BankersDetails or
                OnboardingScreenConstant.BusinessDetails or
                OnboardingScreenConstant.DistBusinessDetails or
                OnboardingScreenConstant.EarlierWorkWithCMI or
                OnboardingScreenConstant.AreaOfDistAgreed or
                OnboardingScreenConstant.AreaofOperationAgreed => typeof(StoreAdditionalInfoCMI), // StoreAdditionalInfoCMI model class

                // Grouping cases for Address model
                OnboardingScreenConstant.BilltoAddress or
                OnboardingScreenConstant.ShiptoAddress => typeof(Winit.Modules.Address.Model.Classes.Address), // Address model class for both billing and shipping

                // Grouping Contact model
                OnboardingScreenConstant.Contact => typeof(Contact), // Contact model class

                // Handle default case
                _ => throw new ArgumentOutOfRangeException(nameof(sectionName), $"No model found for section: {sectionName}") // Throwing an exception for unmatched cases
            };
        }



        // Method to get field name using the model type (reflection) and column attribute
        public string GetFieldDisplayName(string fieldName, string sectionName)
        {
            var modelType = GetModelType(sectionName);

            if (modelType != null)
            {
                var property = modelType.GetProperties()
                                        .FirstOrDefault(p => p.GetCustomAttributes(typeof(ColumnAttribute), false)
                                                             .Cast<ColumnAttribute>()
                                                             .Any(attr => attr.Name == fieldName));

                if (property != null)
                {
                    return FormatPropertyName(property.Name); // Format the property name before returning
                }
            }

            // If no attribute is found, format the field name (e.g., store_size -> Store Size)
            return string.Join(" ", fieldName.Split('_').Select(part => char.ToUpper(part[0]) + part.Substring(1)));
        }
        public string FormatPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return propertyName; // Return as is if null or empty
            }

            // Insert a space before each uppercase letter (except for the first character)
            var formattedName = string.Concat(propertyName.Select((ch, index) =>
                index > 0 && char.IsUpper(ch) ? " " + ch : ch.ToString()));

            return formattedName; // Return the formatted name
        }

        public string GetTableName(string sectionName)
        {
            switch (sectionName)
            {
                case OnboardingScreenConstant.CustomInfoStore:
                    return "store";

                case OnboardingScreenConstant.Contact:
                    return "contact";
                case OnboardingScreenConstant.CustomInfoStoreAdditionInfo:
                    return "store_additional_info";

                case OnboardingScreenConstant.EmployeeDetails:
                case OnboardingScreenConstant.ServiceCenterDetail:
                case OnboardingScreenConstant.ShowroomDetails:
                case OnboardingScreenConstant.BankersDetails:
                case OnboardingScreenConstant.BusinessDetails:
                case OnboardingScreenConstant.DistBusinessDetails:
                case OnboardingScreenConstant.EarlierWorkWithCMI:
                case OnboardingScreenConstant.AreaOfDistAgreed:
                case OnboardingScreenConstant.AreaofOperationAgreed:
                case OnboardingScreenConstant.CustomInfoStoreAdditionInfoCmi:
                    return "store_additional_info_cmi";

                case OnboardingScreenConstant.BilltoAddress:
                case OnboardingScreenConstant.ShiptoAddress:
                    return "address";

                default:
                    return default;
            }
        }
        #region Approval Logic

        private ApprovalEngine? childComponentRef;

        private async Task<ApprovalActionResponse> HandleApprovalAction(ApprovalStatusUpdate approvalStatusUpdate)
        {
            ApprovalActionResponse approvalActionResponse = new ApprovalActionResponse();
            approvalActionResponse.IsApprovalActionRequired=false;
            try
            {
                if (approvalStatusUpdate.IsFinalApproval || approvalStatusUpdate.Status == ApprovalConst.Rejected)
                {
                    _ApprovalEngine.DisplayChangeRequestApproval.Status=approvalStatusUpdate.Status;
                    ApiResponse<string> apiResponse = await _ApprovalEngine.UpdateChangesInMainTable();
                }
                return approvalActionResponse;
            }
            catch (CustomException ex)
            {
                return approvalActionResponse;
            }
        }
        #endregion
    }
}
