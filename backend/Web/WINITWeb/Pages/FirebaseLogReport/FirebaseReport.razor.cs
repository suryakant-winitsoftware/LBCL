using System.Globalization;
using System.Resources;
using System.Security.Policy;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.UIComponents.Common.Language;
using Winit.UIComponents.Web.Filter;
using Winit.UIModels.Common.Filter;
using Winit.UIModels.Web.Breadcrum.Interfaces;
using Winit.UIModels.Web.Breadcrum.Classes;


namespace WinIt.Pages.FirebaseLogReport;

partial class FirebaseReport
{
    public List<DataGridColumn> DataGridColumns { get; set; }
    public List<FilterModel> FilterColumns { get; set; } = new List<FilterModel>();
    private Filter? FilterRef;
    public bool IsInitialized { get; set; }
    private List<ISelectionItem> IsFailureSelectionItems = new List<ISelectionItem>
    {
        new SelectionItem{ UID = "1",Code="Yes",Label="Yes"},
        new SelectionItem{ UID = "0",Code="No",Label="No"},
    };
    IDataService dataService = new DataServiceModel()
    {
        HeaderText = "Maintain Fire Base Log Report",
        BreadcrumList = new List<IBreadCrum>()
         {
             new BreadCrumModel(){SlNo=1,Text="Maintain Fire Base Log Report"},
         }
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            ShowLoader();
            LoadResources(null, _languageService.SelectedCulture);
            await _firebaseReportViewModel.LoadFirebaseData();
            PrepareGrid();
            PrepareFilter();
            IsInitialized = true;
            HideLoader();
        }
        catch (Exception ex)
        {
            HideLoader();
            throw;
        }
    }
    
    private void PrepareGrid()
    {
        DataGridColumns = new List<DataGridColumn>
            {
                new DataGridColumn { Header = @Localizer["uid"], GetValue = s => ((IFirebaseReport)s).UID },
                new DataGridColumn { Header = @Localizer["request_type"], GetValue = s => ((IFirebaseReport)s).LinkedItemType },
                new DataGridColumn { Header = @Localizer["request_created_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestCreatedTime)},
                new DataGridColumn { Header = @Localizer["request_posted_to_api_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestPosted2ApiTime)},
                new DataGridColumn { Header = @Localizer["request_received_by_api_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestReceivedByApiTime)},
                new DataGridColumn { Header = @Localizer["request_sent_to_service_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestSent2ServiceTime)},
                new DataGridColumn { Header = @Localizer["request_received_by_service_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestReceivedByServiceTime)},
                new DataGridColumn { Header = @Localizer["request_posted_to_db_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestPostedToDBTime)},
                new DataGridColumn { Header = @Localizer["notification_sent_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).NotificationSentTime)},
                new DataGridColumn { Header = @Localizer["notification_received_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).NotificationReceivedTime)},
                new DataGridColumn { Header = @Localizer["notification_sent_to_log_api_time"], GetValue = s => CommonFunctions.GetDateTimeInFormat(((IFirebaseReport)s).RequestSent2LogApiTime)},
                new DataGridColumn { Header = @Localizer["is_failed"], GetValue = s => ((IFirebaseReport)s).IsFailed },
                new DataGridColumn { Header = @Localizer["modified_time"], GetValue = s => ((IFirebaseReport)s).ModifiedTime },
                new DataGridColumn { Header = @Localizer["action"],  IsButtonColumn = true, ButtonActions  = new List<ButtonAction>{
                new ButtonAction{ URL = "Images/view.png",ButtonType = ButtonTypes.Image,Action=(item) => _navigationManager.NavigateTo($"FireBaseDetails?FirebaseReportUID={((IFirebaseReport)item).LinkedItemUID}")},
                 } }
            };
    }
    private void PrepareFilter()
    {
        FilterColumns.AddRange(new List<FilterModel> {
            new () { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label = @Localizer["uid"],
                ColumnName = "UID" },
            new () { FilterType = Winit.Shared.Models.Constants.FilterConst.TextBox, Label =@Localizer["request_type"],
                ColumnName = "LinkedItemType" },
            new () { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["request_created_from_date"],
                ColumnName = "RequestCreatedTime" },
            new () { FilterType = Winit.Shared.Models.Constants.FilterConst.Date, Label = @Localizer["request_created_to_date"],
                ColumnName = "RequestCreatedTime" },
            new () { FilterType = Winit.Shared.Models.Constants.FilterConst.DropDown, Label = @Localizer["is_failed"],
                ColumnName = "IsFailed" ,DropDownValues = IsFailureSelectionItems},
        });
    }
    private async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
    {
        await _firebaseReportViewModel.ApplyFilter(keyValuePairs);
    }
}
