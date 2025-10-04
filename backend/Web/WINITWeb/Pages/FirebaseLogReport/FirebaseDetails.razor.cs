using System.Globalization;
using System.Resources;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;

using Winit.UIComponents.Common.Language;

namespace WinIt.Pages.FirebaseLogReport
{
    partial class FirebaseDetails
    {
        public string FirebaseReportUID { get; set; }
        public bool IsInitialized { get; set; }
        IDataService dataService = new DataServiceModel()
        {
            HeaderText = "Maintain Fire Base Log Report Details",
            BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Maintain Fire Base Log Reports ",URL="FirebaseLogReport", IsClickable=true},
            new BreadCrumModel(){SlNo=1,Text="Maintain Fire Base Log Report Details"},

         }
        };

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LoadResources(null, _languageService.SelectedCulture);
                ShowLoader();
                GetQueryParamValue();
                await _firebaseReportViewModel.GetFirebaseDetails(FirebaseReportUID);                
                IsInitialized = true;
                HideLoader();
            }
            catch (Exception ex)
            {
                HideLoader();
                throw;
            }
        }
       
        private async Task RepostFirebaseLog()
        {
            try {
                int retValue = 0;
                retValue = await _firebaseReportViewModel.RepostFirebaseLog(_firebaseReportViewModel.FirebaseDetails);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void GetQueryParamValue()
        {
            var uri = new Uri(NavigationManager.Uri);
            var query = uri.Query.TrimStart('?');
            var queryParams = query.Split('&')
                                   .Select(param => param.Split('='))
                                   .ToDictionary(pair => pair[0], pair => pair.Length > 1 ? pair[1] : "");

            if (queryParams.ContainsKey("FirebaseReportUID"))
            {
                FirebaseReportUID = queryParams["FirebaseReportUID"];
            }
        }
    }
}
