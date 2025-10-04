using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.BL;
using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.FirebaseReport.BL.Interfaces;
using Winit.Modules.FirebaseReport.Models.Interfaces;
using Winit.Shared.Models;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.FirebaseReport.BL.Classes;


public abstract class FirebaseReportBaseViewModel : IFirebaseReportViewModel
{
    public List<IFirebaseReport> FirebaseDataReports { get; set; }
    public IFirebaseReport FirebaseDetails { get; set; }
    protected readonly IAppUser _appUser;
    public List<FilterCriteria> FilterCriterias { get; set; }

    public FirebaseReportBaseViewModel(IAppUser appUser)
    {
        _appUser = appUser;
        FirebaseDataReports = new List<IFirebaseReport>();
        FilterCriterias = new List<FilterCriteria>();
    }

    public async Task LoadFirebaseData()
    {
        FirebaseDataReports.Clear();    
        var data = await GetFirebaseData();
        if (data is not null)
        {
            FirebaseDataReports.AddRange(data);
        }
    }

    public async Task GetFirebaseDetails(string UID)
    {
        FirebaseDetails = null;
        var data = await GetFirebaseDetailsData(UID);
        if (data is not null)
        {
            FirebaseDetails = data;
        }
    }

    public async Task LoadFirebaseDetails(string UID)
    {
        await LoadFirebaseData();
        var data = FirebaseDataReports.FirstOrDefault(report => report.LinkedItemUID == UID);
        if (data is not null)
        {
            FirebaseDetails = data;
        }
    }
    public async Task ApplyFilter(Dictionary<string, string> keyValuePairs)
    {
        FilterCriterias.Clear();
        foreach (var item in keyValuePairs)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                if (item.Key == "IsFailed")
                {
                    bool IsFailed = false;
                    if(item.Value == "1") IsFailed = true; 
                    FilterCriterias.Add(new FilterCriteria(item.Key, IsFailed, FilterType.Equal));
                }
                else
                {
                    FilterCriterias.Add(new FilterCriteria(item.Key, item.Value, FilterType.Equal));
                }
            }
        }
        await LoadFirebaseData();
    }
    public abstract Task<int> RepostFirebaseLog(IFirebaseReport FirebaseReport);

    public abstract Task<IFirebaseReport?> GetFirebaseDetailsData(string UID);
    public abstract Task<List<IFirebaseReport>?> GetFirebaseData();

    /*public abstract Task<List<string>?> GetTypeFilterData();
    public virtual async Task<int> RepostFirebaseLog()
    {
        LogEntry logEntry = new LogEntry();
        logEntry.UID = FirebaseDetails.UID;
        logEntry.LinkedItemUID = FirebaseDetails.LinkedItemUID;
        logEntry.NextUID = FirebaseDetails.NextUID;
        logEntry.LinkedItemType = FirebaseDetails.LinkedItemType;
        logEntry.RequestBody = FirebaseDetails.RequestBody;
        logEntry.RequestCreatedTime = FirebaseDetails.RequestCreatedTime.ToString();
        logEntry.RequestPostedToApiTime = FirebaseDetails.RequestPostedToApiTime.ToString();
        logEntry.RequestSent2LogApiTime = FirebaseDetails.RequestSent2LogApiTime.ToString();
        logEntry.NotificationReceivedTime = FirebaseDetails.NotificationReceivedTime.ToString();
        logEntry.AppComments = FirebaseDetails.AppComments;
        logEntry.IsFailed = FirebaseDetails.IsFailed.ToString();
        logEntry.ModifiedTime = FirebaseDetails.ModifiedTime.ToString();

        return await RepostFirebaseLog_Data(logEntry);
    }
    public abstract Task<int> RepostFirebaseLog_Data(LogEntry logEntry);*/


}