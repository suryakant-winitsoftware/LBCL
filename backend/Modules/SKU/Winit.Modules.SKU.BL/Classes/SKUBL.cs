using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.DL.Interfaces;
using Winit.Modules.SKU.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;
using Winit.Shared.CommonUtilities.Extensions;
using Winit.Modules.SKU.Model.Interfaces;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Winit.Modules.SKU.BL.Classes;

public class SKUBL : ISKUBL
{
    protected readonly DL.Interfaces.ISKUDL _skuDL = null;
    IServiceProvider _serviceProvider = null;
    public SKUBL(DL.Interfaces.ISKUDL skuDL, IServiceProvider serviceProvider)
    {
        _skuDL = skuDL;
        _serviceProvider = serviceProvider;
    }
    public async Task<PagedResponse<Winit.Modules.SKU.Model.Interfaces.ISKU>> SelectAllSKUDetails(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _skuDL.SelectAllSKUDetails(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }
    public async Task<Winit.Modules.SKU.Model.Interfaces.ISKU> SelectSKUByUID(string UID)
    {
        return await _skuDL.SelectSKUByUID(UID);
    }
    public async Task<int> CreateSKU(Winit.Modules.SKU.Model.Interfaces.ISKUV1 sKU)
    {
        return await _skuDL.CreateSKU(sKU);
    }

    public async Task<int> UpdateSKU(Winit.Modules.SKU.Model.Interfaces.ISKU sKU)
    {
        return await _skuDL.UpdateSKU(sKU);
    }

    public async Task<int> DeleteSKU(string UID)
    {
        return await _skuDL.DeleteSKU(UID);
    }
    //public async Task<List<Model.Interfaces.ISKUMaster>> PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs,
    //    List<string> skuUIDs, List<string> attributeTypes)
    //{
    //    List<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>? sKUMasters = null;
    //    var (skuList,skuConfigList,skuUomList,sKUAttributesList,taxSkuMapList) = await _skuDL.PrepareSKUMaster(orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes);

    //    if (skuList != null && skuList.Count > 0)
    //    {
    //        sKUMasters = new List<Model.Interfaces.ISKUMaster>();
    //        foreach (var sku in skuList)
    //        {
    //            Winit.Modules.SKU.Model.Interfaces.ISKUMaster sKUMaster = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>();
    //            sKUMaster.SKU = sku;
    //            sKUMaster.SKUConfigs = skuConfigList.Where(e => e.SKUUID == sku.UID).ToList();
    //            if(sKUMaster.SKUConfigs == null || sKUMaster.SKUConfigs.Count() == 0)
    //            {
    //                continue;
    //            }
    //            sKUMaster.SKUUOMs = skuUomList.Where(e => e.SKUUID == sku.UID).ToList();
    //            if (sKUMaster.SKUUOMs == null || sKUMaster.SKUUOMs.Count() == 0 || 
    //                !(sKUMaster.SKUUOMs.Any(e=>e.IsBaseUOM) && sKUMaster.SKUUOMs.Any(e => e.IsOuterUOM)))
    //            {
    //                continue;
    //            }
    //            sKUMaster.SKUAttributes = sKUAttributesList.Where(e => e.SKUUID == sku.UID).ToList();
    //            sKUMaster.ApplicableTaxUIDs = taxSkuMapList.Where(e => e.SKUUID == sku.UID).Select(e=>e.TaxUID).ToList();
    //            sKUMasters.Add(sKUMaster);
    //        }
    //    }
    //    return sKUMasters;
    //}

    // public async Task<List<Model.Interfaces.ISKUMaster>> PrepareSKUMaster(List<string> orgUIDs, List<string> DistributionChannelUIDs,
    //List<string> skuUIDs, List<string> attributeTypes)
    // {
    //     List<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>? sKUMasters = null;

    //     var (skuList, skuConfigList, skuUomList, sKUAttributesList, taxSkuMapList) = await MeasureExecutionTimeAsyncWithReturn("PrepareSKUMaster_Data (Query )",
    //         () => _skuDL.PrepareSKUMaster(orgUIDs, DistributionChannelUIDs, skuUIDs, attributeTypes));

    //     if (skuList != null && skuList.Count > 0)
    //     {
    //         sKUMasters = new List<Model.Interfaces.ISKUMaster>();

    //         MeasureExecutionTime("ForeachLoop", () =>
    //         {
    //             foreach (var sku in skuList)
    //             {
    //                 Winit.Modules.SKU.Model.Interfaces.ISKUMaster sKUMaster = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>();
    //                 sKUMaster.SKU = sku;
    //                 sKUMaster.SKUConfigs = skuConfigList.Where(e => e.SKUUID == sku.UID).ToList();
    //                 if (sKUMaster.SKUConfigs == null || sKUMaster.SKUConfigs.Count == 0)
    //                 {
    //                     continue;
    //                 }
    //                 sKUMaster.SKUUOMs = skuUomList.Where(e => e.SKUUID == sku.UID).ToList();
    //                 if (sKUMaster.SKUUOMs == null || sKUMaster.SKUUOMs.Count == 0 ||
    //                     !(sKUMaster.SKUUOMs.Any(e => e.IsBaseUOM) && sKUMaster.SKUUOMs.Any(e => e.IsOuterUOM)))
    //                 {
    //                     continue;
    //                 }
    //                 sKUMaster.SKUAttributes = sKUAttributesList.Where(e => e.SKUUID == sku.UID).ToList();
    //                 sKUMaster.ApplicableTaxUIDs = taxSkuMapList.Where(e => e.SKUUID == sku.UID).Select(e => e.TaxUID).ToList();
    //                 sKUMasters.Add(sKUMaster);
    //             }
    //         });
    //     }
    //     return sKUMasters;
    // }
    public async Task<List<Model.Interfaces.ISKUMaster>> PrepareSKUMaster(
    List<string> orgUIDs,
    List<string> distributionChannelUIDs,
    List<string> skuUIDs,
    List<string> attributeTypes)
    {
        var (skuList, skuConfigList, skuUomList, sKUAttributesList, taxSkuMapList) =
            await _skuDL.PrepareSKUMaster(orgUIDs, distributionChannelUIDs, skuUIDs, attributeTypes);

        var skuConfigLookup = skuConfigList?.ToLookup(e => e.SKUUID);
        var skuUomLookup = skuUomList?.ToLookup(e => e.SKUUID);
        var skuAttrLookup = sKUAttributesList?.ToLookup(e => e.SKUUID);
        var taxMapLookup = taxSkuMapList?.ToLookup(e => e.SKUUID);

        var sKUMasters = new ConcurrentBag<Model.Interfaces.ISKUMaster>();
        if (skuList != null && skuList.Any())
        {

            Parallel.ForEach(skuList, sku =>
            {
                var sKUMaster = _serviceProvider.CreateInstance<Model.Interfaces.ISKUMaster>();
                sKUMaster.SKU = sku;
                sKUMaster.SKUConfigs = skuConfigLookup?[sku.UID]?.ToList();
                sKUMaster.SKUUOMs = skuUomLookup?[sku.UID]?.ToList();
                sKUMaster.SKUAttributes = skuAttrLookup?[sku.UID]?.ToList();
                sKUMaster.ApplicableTaxUIDs = taxMapLookup?[sku.UID]?.Select(e => e.TaxUID).ToList();

                if (sku is ISKUV1 skuv1 && sKUMaster.SKUAttributes != null)
                {
                    var keys = new HashSet<string>(sKUMaster.SKUAttributes.Select(e => e.Code + "_" + e.Type));
                    if (!string.IsNullOrEmpty(sku.OrgUID)) keys.Add(sku.OrgUID + "_ORG");
                    if (!string.IsNullOrEmpty(sku.SupplierOrgUID)) keys.Add(sku.SupplierOrgUID + "_Division");
                    skuv1.FilterKeys = keys;
                }

                sKUMasters.Add(sKUMaster);
            });
        }

        return sKUMasters.ToList();
    }




    public async Task<PagedResponse<ISKUListView>> SelectAllSKUDetailsWebView(List<SortCriteria> sortCriterias, int pageNumber,
        int pageSize, List<FilterCriteria> filterCriterias, bool isCountRequired)
    {
        return await _skuDL.SelectAllSKUDetailsWebView(sortCriterias, pageNumber, pageSize, filterCriterias, isCountRequired);
    }


    public async Task<Model.Interfaces.ISKUMaster> SelectSKUMasterByUID(string UID)
    {
        Winit.Modules.SKU.Model.Interfaces.ISKUMaster sKUMaster = null;
        var (skuList, skuConfigList, skuUomList, sKUAttributesList, CustomSKUFieldsList, fileSysList) = await _skuDL.SelectSKUMasterByUID(UID);

        if (skuList != null && skuList.Count > 0)
        {
            sKUMaster = _serviceProvider.CreateInstance<Winit.Modules.SKU.Model.Interfaces.ISKUMaster>();
            sKUMaster.SKU = skuList.FirstOrDefault();
            sKUMaster.SKUConfigs = skuConfigList;
            sKUMaster.SKUUOMs = skuUomList;
            sKUMaster.SKUAttributes = sKUAttributesList;
            sKUMaster.CustomSKUFields = CustomSKUFieldsList;
            sKUMaster.FileSysList = fileSysList;
        }
        return sKUMaster;
    }
    public List<string> _methodTimes = new List<string>();
    public void MeasureExecutionTime(string methodName, Action method)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        method();
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        _methodTimes.Add($"{methodName}: {timeTaken.TotalSeconds} seconds");
    }
    public async Task MeasureExecutionTimeAsync(string methodName, Func<Task> method)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await method();
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        _methodTimes.Add($"{methodName}: {timeTaken.TotalSeconds} seconds");
    }
    public T MeasureExecutionTimeWithReturn<T>(string methodName, Func<T> method)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        T result = method();
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        _methodTimes.Add($"{methodName}: {timeTaken.TotalSeconds} seconds");
        return result;
    }

    public async Task<T> MeasureExecutionTimeAsyncWithReturn<T>(string methodName, Func<Task<T>> method)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        T result = await method();
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;
        _methodTimes.Add($"{methodName}: {timeTaken.TotalSeconds} seconds");
        return result;
    }
    public List<string> GetMethodExecutionTimesfromSkuBl()
    {
        return _methodTimes;
    }

    public async Task<int> CRUDWinitCache(string key, string value, System.Data.IDbConnection? connection = null, System.Data.IDbTransaction? transaction = null)
    {
        return await _skuDL.CRUDWinitCache(key, value, connection, transaction);
    }

    public async Task<List<ISKUMaster>> GetWinitCache(string key, System.Data.IDbConnection? connection = null, System.Data.IDbTransaction? transaction = null)
    {
        return await _skuDL.GetWinitCache(key, connection, transaction);
    }
    public async Task<Dictionary<string, List<string>>> GetLinkedItemUIDByStore(string linkedItemType, List<string> storeUIDs)
    {
        return await _skuDL.GetLinkedItemUIDByStore(linkedItemType, storeUIDs);
    }


}
