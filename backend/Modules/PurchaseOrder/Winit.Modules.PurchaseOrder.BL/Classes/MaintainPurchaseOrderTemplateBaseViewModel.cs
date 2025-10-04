using Winit.Modules.Common.BL.Interfaces;
using Winit.Modules.PurchaseOrder.BL.Interfaces;
using Winit.Modules.PurchaseOrder.Model.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.PurchaseOrder.BL.Classes;

public class MaintainPurchaseOrderTemplateBaseViewModel : IMaintainPurchaseOrderTemplateViewModel
{
    public List<FilterCriteria> FilterCriterias { get; set; }
    public List<SortCriteria> SortCriterias { get; set; }
    public List<IPurchaseOrderTemplateHeader> PurchaseOrderTemplateHeaders { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string Status { get; set; }


    //DI
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppUser _appUser;
    private readonly IAppSetting _appSetting;
    private readonly IAppConfig _appConfigs;
    private readonly IMaintainPurchaseOrderTemplateDataHelper _maintainPurchaseOrderTemplateDataHelper;

    public MaintainPurchaseOrderTemplateBaseViewModel(IServiceProvider serviceProvider,
        IAppUser appUser,
        IAppSetting appSetting,
        IAppConfig appConfigs,
        IMaintainPurchaseOrderTemplateDataHelper maintainPurchaseOrderTemplateDataHelper)
    {
        _serviceProvider = serviceProvider;
        _appUser = appUser;
        _appSetting = appSetting;
        _appConfigs = appConfigs;
        _maintainPurchaseOrderTemplateDataHelper = maintainPurchaseOrderTemplateDataHelper;
        PurchaseOrderTemplateHeaders = [];
        FilterCriterias = [];
        SortCriterias = [];
    }

    public async Task PopulateViewModel()
    {
        PurchaseOrderTemplateHeaders.Clear();
        FilterCriteria? filterCriteria = FilterCriterias.Find(e => "createdby".Equals(e.Name, StringComparison.OrdinalIgnoreCase));
        if (filterCriteria == null)
        {
            FilterCriterias.Add(new FilterCriteria("createdby", _appUser.Emp.UID, FilterType.Equal));
        }
        PagedResponse<IPurchaseOrderTemplateHeader>? data = await _maintainPurchaseOrderTemplateDataHelper
            .GetAllPurchaseOrderTemplateHeader(PageNumber, PageSize, SortCriterias, FilterCriterias, true);
        if (data is not null)
        {
            TotalCount = data.TotalCount;
            PurchaseOrderTemplateHeaders.AddRange(data.PagedData);
        }
    }

    public async Task PageIndexChanged(int index)
    {
        PageNumber = index;
        await PopulateViewModel();
    }
    public async Task OnFilterApply(Dictionary<string, string> keyValuePairs)
    {
        try
        {
            FilterCriterias.Clear();
            if ((keyValuePairs.TryGetValue("templatename", out string templateName) 
                && !string.IsNullOrEmpty(templateName))
                || (keyValuePairs.TryGetValue("isactive", out string isActive) 
                && isActive.ToLower() != "false"))
            {
                foreach (var keyValue in keyValuePairs)
                {
                    if (!string.IsNullOrEmpty(keyValue.Value))
                    {
                        if (keyValue.Value.Contains(","))
                        {
                            string[] values = keyValue.Value.Split(',');
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, values, FilterType.In));
                        }
                        else if (keyValue.Key == "isactive")
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Equal));
                        }
                        else
                        {
                            FilterCriterias.Add(new FilterCriteria(keyValue.Key, keyValue.Value, FilterType.Like));
                        }
                    }
                }
            }
            await PopulateViewModel();
        }
        catch (Exception ex)
        {

        }
    }
    public async Task OnSortClick(SortCriteria sortCriteria)
    {
        SortCriterias.Clear();
        SortCriterias.Add(sortCriteria);
        await PopulateViewModel();
    }

    public async Task OnDeleteClick(List<string> purchaseOrderTemplateHeaderUids)
    {
        try
        {
            if (await _maintainPurchaseOrderTemplateDataHelper.DeletePurchaseOrderHeaderByUIDs(purchaseOrderTemplateHeaderUids))
            {
                PurchaseOrderTemplateHeaders.RemoveAll(e => purchaseOrderTemplateHeaderUids.Contains(e.UID));
                throw new CustomException(ExceptionStatus.Success, "Template deleted successfully..");
            }
            else
            {
                throw new CustomException(ExceptionStatus.Failed, "Template deleting failed..");
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase))
            {
                throw new CustomException(ExceptionStatus.Failed, "This template cannot be deleted because it is currently being used by a Purchase Order. Please update or remove its usage before attempting to delete it.");
            }
            else if (e is CustomException && (e as CustomException).Status == ExceptionStatus.Success)
            {
                throw new CustomException(ExceptionStatus.Success, e.Message);
            }
            else
            {
                throw new CustomException(ExceptionStatus.Failed, "Template deleting failed..");
            }
        }
    }
}
