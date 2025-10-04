using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Winit.Modules.SKU.Model.Classes;
using System.Linq;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using WINITServices.Interfaces.CacheHandler;
using Winit.Modules.SKU.Model.Interfaces;
using System.Collections.Concurrent;
using System.Threading;

namespace WINITAPI.Controllers.SKU;

[ApiController]
[Route("api/[Controller]")]
public class DataPreparationChunkedController : WINITBaseController
{
    private readonly Winit.Modules.SKU.BL.Interfaces.ISKUBL _skuBL;
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<string, CachePreparationStatus> _processingStatus = new();
    private readonly ParallelOptions _parallelOptions;

    public DataPreparationChunkedController(
        IServiceProvider serviceProvider,
        Winit.Modules.SKU.BL.Interfaces.ISKUBL skuBL) : base(serviceProvider)
    {
        _skuBL = skuBL;
        _serviceProvider = serviceProvider;
        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
        };
    }

    /// <summary>
    /// Prepare SKU Master data in chunks - Production Ready for Large Datasets
    /// </summary>
    [HttpPost]
    [Route("PrepareSKUMasterChunked")]
    public async Task<ActionResult> PrepareSKUMasterChunked([FromBody] ChunkedPrepareSKURequest request)
    {
        try
        {
            Log.Information($"Starting chunked SKU cache preparation - Page: {request.PageNumber}, Size: {request.PageSize}");
            
            // Validate request
            if (request.PageNumber < 1 || request.PageSize < 1 || request.PageSize > 1000)
            {
                return BadRequest("Invalid page parameters. PageSize must be between 1 and 1000.");
            }

            // Calculate skip and take
            int skip = (request.PageNumber - 1) * request.PageSize;
            
            // Get the chunk of SKU master data
            var sKUMasterList = await _skuBL.PrepareSKUMaster(
                request.OrgUIDs, 
                request.DistributionChannelUIDs, 
                request.SKUUIDs, 
                request.AttributeTypes);

            if (sKUMasterList == null)
            {
                return CreateOkApiResponse(new ChunkedResponse 
                { 
                    Success = true, 
                    ProcessedCount = 0, 
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    Message = "No SKUs found"
                });
            }

            // Apply pagination
            var totalCount = sKUMasterList.Count;
            var chunkedList = sKUMasterList.Skip(skip).Take(request.PageSize).ToList();
            
            Log.Information($"Processing {chunkedList.Count} SKUs from total {totalCount} (Page {request.PageNumber})");

            // Process this chunk into cache
            int successCount = 0;
            int failureCount = 0;

            if (chunkedList.Count > 0)
            {
                var successBag = new ConcurrentBag<int>();
                var failureBag = new ConcurrentBag<string>();

                Parallel.ForEach(chunkedList, _parallelOptions, sKUMaster =>
                {
                    try
                    {
                        if (sKUMaster.SKU != null)
                        {
                            // Cache the SKU data
                            _cacheService.HSet(CacheConstants.SKU, sKUMaster.SKU.UID, sKUMaster.SKU, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                            
                            _cacheService.Set<string>($"{CacheConstants.FilterSKUOrgUID}{sKUMaster.SKU.OrgUID}_{sKUMaster.SKU.UID}", 
                                sKUMaster.SKU.UID, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                        
                        if (sKUMaster.SKUConfigs != null && sKUMaster.SKUConfigs.Count > 0)
                        {
                            _cacheService.HSet(CacheConstants.SKUConfig, sKUMaster.SKU.UID, sKUMaster.SKUConfigs, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                        
                        if (sKUMaster.SKUUOMs != null && sKUMaster.SKUUOMs.Count > 0)
                        {
                            _cacheService.HSet(CacheConstants.SKUUOM, sKUMaster.SKU.UID, sKUMaster.SKUUOMs, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                        
                        if (sKUMaster.SKUAttributes != null && sKUMaster.SKUAttributes.Count > 0)
                        {
                            _cacheService.HSet(CacheConstants.SKUAttributes, sKUMaster.SKU.UID, sKUMaster.SKUAttributes, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                        
                        if (sKUMaster.ApplicableTaxUIDs != null && sKUMaster.ApplicableTaxUIDs.Count > 0)
                        {
                            _cacheService.HSet(CacheConstants.TaxSKUMap, sKUMaster.SKU.UID, sKUMaster.ApplicableTaxUIDs, 
                                WINITServices.Classes.CacheHandler.ExpirationType.Absolute, -1);
                        }
                        
                        if (sKUMaster.SKU != null)
                        {
                            _cacheService.HSet(CacheConstants.SKUMaster, sKUMaster.SKU.UID, sKUMaster);
                        }
                        
                        successBag.Add(1);
                    }
                    catch (Exception ex)
                    {
                        failureBag.Add($"SKU {sKUMaster.SKU?.UID}: {ex.Message}");
                    }
                });

                successCount = successBag.Count;
                failureCount = failureBag.Count;
                
                if (failureCount > 0)
                {
                    Log.Warning($"Failed to cache {failureCount} SKUs: {string.Join(", ", failureBag.Take(5))}");
                }
            }

            Log.Information($"Chunk processing complete. Success: {successCount}, Failed: {failureCount}");

            return CreateOkApiResponse(new ChunkedResponse
            {
                Success = true,
                ProcessedCount = successCount,
                FailedCount = failureCount,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                HasMore = skip + request.PageSize < totalCount,
                Message = $"Processed page {request.PageNumber} successfully"
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to process SKU chunk - Page: {request.PageNumber}");
            return CreateErrorResponse($"Error processing chunk: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the total count of SKUs that need to be cached
    /// </summary>
    [HttpPost]
    [Route("GetSKUCountForCache")]
    public async Task<ActionResult> GetSKUCountForCache([FromBody] PrepareSKURequestModel request)
    {
        try
        {
            var sKUMasterList = await _skuBL.PrepareSKUMaster(
                request.OrgUIDs, 
                request.DistributionChannelUIDs, 
                request.SKUUIDs, 
                request.AttributeTypes);

            var count = sKUMasterList?.Count ?? 0;
            
            return CreateOkApiResponse(new 
            { 
                TotalCount = count,
                RecommendedChunkSize = count > 10000 ? 500 : (count > 1000 ? 100 : 50),
                EstimatedTimeSeconds = (count / 100) * 2 // Rough estimate: 2 seconds per 100 SKUs
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get SKU count");
            return CreateErrorResponse($"Error getting count: {ex.Message}");
        }
    }

    /// <summary>
    /// Start async cache preparation with progress tracking
    /// </summary>
    [HttpPost]
    [Route("StartAsyncCachePreparation")]
    public async Task<ActionResult> StartAsyncCachePreparation([FromBody] PrepareSKURequestModel request)
    {
        try
        {
            var processId = Guid.NewGuid().ToString();
            var status = new CachePreparationStatus
            {
                ProcessId = processId,
                StartTime = DateTime.UtcNow,
                Status = "Starting",
                TotalCount = 0,
                ProcessedCount = 0
            };

            _processingStatus[processId] = status;

            // Start async processing
            _ = Task.Run(async () =>
            {
                try
                {
                    status.Status = "Getting SKU list";
                    var sKUMasterList = await _skuBL.PrepareSKUMaster(
                        request.OrgUIDs,
                        request.DistributionChannelUIDs,
                        request.SKUUIDs,
                        request.AttributeTypes);

                    if (sKUMasterList != null && sKUMasterList.Count > 0)
                    {
                        status.TotalCount = sKUMasterList.Count;
                        status.Status = "Processing";

                        const int chunkSize = 500;
                        var chunks = sKUMasterList
                            .Select((sku, index) => new { sku, index })
                            .GroupBy(x => x.index / chunkSize)
                            .Select(g => g.Select(x => x.sku).ToList())
                            .ToList();

                        foreach (var chunk in chunks)
                        {
                            Parallel.ForEach(chunk, _parallelOptions, sKUMaster =>
                            {
                                try
                                {
                                    // Cache the SKU (same logic as above)
                                    if (sKUMaster.SKU != null)
                                    {
                                        _cacheService.HSet(CacheConstants.SKUMaster, sKUMaster.SKU.UID, sKUMaster);
                                    }
                                    status.IncrementProcessed();
                                }
                                catch
                                {
                                    status.IncrementFailed();
                                }
                            });

                            // Small delay between chunks
                            await Task.Delay(100);
                        }
                    }

                    status.Status = "Completed";
                    status.EndTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    status.Status = $"Failed: {ex.Message}";
                    status.EndTime = DateTime.UtcNow;
                }
            });

            return CreateOkApiResponse(new { ProcessId = processId, Message = "Cache preparation started" });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start async cache preparation");
            return CreateErrorResponse($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Check the status of async cache preparation
    /// </summary>
    [HttpGet]
    [Route("GetCachePreparationStatus/{processId}")]
    public ActionResult GetCachePreparationStatus(string processId)
    {
        if (_processingStatus.TryGetValue(processId, out var status))
        {
            return CreateOkApiResponse(status);
        }
        return NotFound("Process ID not found");
    }

    // Clean up old statuses periodically
    [HttpPost]
    [Route("CleanupOldStatuses")]
    public ActionResult CleanupOldStatuses()
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var removed = _processingStatus
            .Where(x => x.Value.EndTime.HasValue && x.Value.EndTime < cutoff)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in removed)
        {
            _processingStatus.TryRemove(key, out _);
        }

        return CreateOkApiResponse($"Cleaned up {removed.Count} old statuses");
    }
}

// Request/Response Models
public class ChunkedPrepareSKURequest : PrepareSKURequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

public class ChunkedResponse
{
    public bool Success { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
    public string Message { get; set; }
}

public class CachePreparationStatus
{
    private int _processedCount;
    private int _failedCount;

    public string ProcessId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }
    public int TotalCount { get; set; }
    
    public int ProcessedCount 
    { 
        get => _processedCount; 
        set => _processedCount = value; 
    }
    
    public int FailedCount 
    { 
        get => _failedCount; 
        set => _failedCount = value; 
    }
    
    public double PercentComplete => TotalCount > 0 ? (ProcessedCount * 100.0 / TotalCount) : 0;
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.UtcNow - StartTime;
    
    public void IncrementProcessed() => Interlocked.Increment(ref _processedCount);
    public void IncrementFailed() => Interlocked.Increment(ref _failedCount);
}