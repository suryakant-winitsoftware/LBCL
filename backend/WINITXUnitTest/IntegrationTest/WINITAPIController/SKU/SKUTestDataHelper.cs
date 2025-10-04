using Moq;
using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace IntegrationTest.WINITAPIController.SKU;

/// <summary>
/// Helper class providing test data and utilities for SKU-related integration tests.
/// Centralizes test data creation to ensure consistency across test methods.
/// </summary>
public static class SKUTestDataHelper
{
    #region Constants

    public const string DEFAULT_SKU_CODE = "TEST001";
    public const string DEFAULT_SKU_NAME = "Test SKU";
    public const string DEFAULT_BASE_UOM = "PCS";
    public const string DEFAULT_HSN_CODE = "12345678";
    public const string DEFAULT_MODEL_CODE = "MODEL001";
    public const int DEFAULT_YEAR = 2024;
    public const int DEFAULT_PRODUCT_CATEGORY_ID = 1;

    #endregion

    #region SKU Creation Methods

    /// <summary>
    /// Creates a mock ISKU object with default or specified values.
    /// </summary>
    /// <param name="uid">The UID for the SKU. If null, generates a new GUID.</param>
    /// <param name="code">The SKU code. If null, uses default value.</param>
    /// <param name="name">The SKU name. If null, uses default value.</param>
    /// <param name="isActive">Whether the SKU is active. Default is true.</param>
    /// <returns>Mock ISKU object</returns>
    public static ISKU CreateMockSKU(string uid = null, string code = null, string name = null, bool isActive = true)
    {
        var mockSKU = new Mock<ISKU>();
        mockSKU.Setup(x => x.UID).Returns(uid ?? Guid.NewGuid().ToString());
        mockSKU.Setup(x => x.Code).Returns(code ?? DEFAULT_SKU_CODE);
        mockSKU.Setup(x => x.Name).Returns(name ?? DEFAULT_SKU_NAME);
        mockSKU.Setup(x => x.IsActive).Returns(isActive);
        mockSKU.Setup(x => x.CompanyUID).Returns(Guid.NewGuid().ToString());
        mockSKU.Setup(x => x.OrgUID).Returns(Guid.NewGuid().ToString());
        mockSKU.Setup(x => x.BaseUOM).Returns(DEFAULT_BASE_UOM);
        mockSKU.Setup(x => x.OuterUOM).Returns("BOX");
        mockSKU.Setup(x => x.IsStockable).Returns(true);
        mockSKU.Setup(x => x.IsThirdParty).Returns(false);
        mockSKU.Setup(x => x.FromDate).Returns(DateTime.Now.AddDays(-30));
        mockSKU.Setup(x => x.ToDate).Returns(DateTime.Now.AddDays(365));
        mockSKU.Setup(x => x.ServerAddTime).Returns(DateTime.Now);
        mockSKU.Setup(x => x.ServerModifiedTime).Returns(DateTime.Now);
        mockSKU.Setup(x => x.ActionType).Returns(ActionType.Add);
        return mockSKU.Object;
    }

    /// <summary>
    /// Creates a mock ISKUV1 object with default or specified values.
    /// </summary>
    /// <param name="uid">The UID for the SKU. If null, generates a new GUID.</param>
    /// <param name="code">The SKU code. If null, uses default value.</param>
    /// <param name="name">The SKU name. If null, uses default value.</param>
    /// <param name="isActive">Whether the SKU is active. Default is true.</param>
    /// <returns>Mock ISKUV1 object</returns>
    public static ISKUV1 CreateMockSKUV1(string uid = null, string code = null, string name = null, bool isActive = true)
    {
        var mockSKUV1 = new Mock<ISKUV1>();
        
        // Set base ISKU properties
        mockSKUV1.Setup(x => x.UID).Returns(uid ?? Guid.NewGuid().ToString());
        mockSKUV1.Setup(x => x.Code).Returns(code ?? DEFAULT_SKU_CODE);
        mockSKUV1.Setup(x => x.Name).Returns(name ?? DEFAULT_SKU_NAME);
        mockSKUV1.Setup(x => x.IsActive).Returns(isActive);
        mockSKUV1.Setup(x => x.CompanyUID).Returns(Guid.NewGuid().ToString());
        mockSKUV1.Setup(x => x.OrgUID).Returns(Guid.NewGuid().ToString());
        mockSKUV1.Setup(x => x.BaseUOM).Returns(DEFAULT_BASE_UOM);
        mockSKUV1.Setup(x => x.IsStockable).Returns(true);
        mockSKUV1.Setup(x => x.ServerAddTime).Returns(DateTime.Now);
        mockSKUV1.Setup(x => x.ServerModifiedTime).Returns(DateTime.Now);
        
        // Set ISKUV1 specific properties
        mockSKUV1.Setup(x => x.HSNCode).Returns(DEFAULT_HSN_CODE);
        mockSKUV1.Setup(x => x.ProductCategoryId).Returns(DEFAULT_PRODUCT_CATEGORY_ID);
        mockSKUV1.Setup(x => x.ModelCode).Returns(DEFAULT_MODEL_CODE);
        mockSKUV1.Setup(x => x.Year).Returns(DEFAULT_YEAR);
        mockSKUV1.Setup(x => x.Type).Returns("Electronics");
        mockSKUV1.Setup(x => x.ProductType).Returns("Consumer");
        mockSKUV1.Setup(x => x.Category).Returns("Appliances");
        mockSKUV1.Setup(x => x.Tonnage).Returns("1.5");
        mockSKUV1.Setup(x => x.Capacity).Returns("100L");
        mockSKUV1.Setup(x => x.StarRating).Returns("5");
        mockSKUV1.Setup(x => x.ProductCategoryName).Returns("Home Appliances");
        mockSKUV1.Setup(x => x.ItemSeries).Returns("Series A");
        mockSKUV1.Setup(x => x.IsAvailableInApMaster).Returns(true);
        mockSKUV1.Setup(x => x.FilterKeys).Returns(new HashSet<string> { "electronics", "appliances" });
        
        return mockSKUV1.Object;
    }

    /// <summary>
    /// Creates a concrete SKU object for testing updates and operations requiring concrete types.
    /// </summary>
    /// <param name="uid">The UID for the SKU. If null, generates a new GUID.</param>
    /// <param name="code">The SKU code. If null, uses default value.</param>
    /// <param name="name">The SKU name. If null, uses default value.</param>
    /// <param name="isActive">Whether the SKU is active. Default is true.</param>
    /// <returns>Concrete SKU object</returns>
    public static Winit.Modules.SKU.Model.Classes.SKU CreateConcreteSKU(string uid = null, string code = null, string name = null, bool isActive = true)
    {
        return new Winit.Modules.SKU.Model.Classes.SKU
        {
            UID = uid ?? Guid.NewGuid().ToString(),
            Code = code ?? DEFAULT_SKU_CODE,
            Name = name ?? DEFAULT_SKU_NAME,
            IsActive = isActive,
            CompanyUID = Guid.NewGuid().ToString(),
            OrgUID = Guid.NewGuid().ToString(),
            BaseUOM = DEFAULT_BASE_UOM,
            OuterUOM = "BOX",
            IsStockable = true,
            IsThirdParty = false,
            FromDate = DateTime.Now.AddDays(-30),
            ToDate = DateTime.Now.AddDays(365),
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            ActionType = ActionType.Update
        };
    }

    /// <summary>
    /// Creates a concrete SKUV1 object for testing.
    /// </summary>
    /// <param name="uid">The UID for the SKU. If null, generates a new GUID.</param>
    /// <param name="code">The SKU code. If null, uses default value.</param>
    /// <param name="name">The SKU name. If null, uses default value.</param>
    /// <param name="isActive">Whether the SKU is active. Default is true.</param>
    /// <returns>Concrete SKUV1 object</returns>
    public static SKUV1 CreateConcreteSKUV1(string uid = null, string code = null, string name = null, bool isActive = true)
    {
        return new SKUV1
        {
            UID = uid ?? Guid.NewGuid().ToString(),
            Code = code ?? DEFAULT_SKU_CODE,
            Name = name ?? DEFAULT_SKU_NAME,
            IsActive = isActive,
            CompanyUID = Guid.NewGuid().ToString(),
            OrgUID = Guid.NewGuid().ToString(),
            BaseUOM = DEFAULT_BASE_UOM,
            IsStockable = true,
            ServerAddTime = DateTime.Now,
            ServerModifiedTime = DateTime.Now,
            HSNCode = DEFAULT_HSN_CODE,
            ProductCategoryId = DEFAULT_PRODUCT_CATEGORY_ID,
            ModelCode = DEFAULT_MODEL_CODE,
            Year = DEFAULT_YEAR,
            Type = "Electronics",
            ProductType = "Consumer",
            Category = "Appliances",
            Tonnage = "1.5",
            Capacity = "100L",
            StarRating = "5",
            ProductCategoryName = "Home Appliances",
            ItemSeries = "Series A",
            IsAvailableInApMaster = true,
            FilterKeys = new HashSet<string> { "electronics", "appliances" }
        };
    }

    #endregion

    #region SKU Master and Related Objects

    /// <summary>
    /// Creates a mock ISKUMaster object with associated data.
    /// </summary>
    /// <param name="uid">The UID for the SKU master. If null, generates a new GUID.</param>
    /// <returns>Mock ISKUMaster object</returns>
    public static ISKUMaster CreateMockSKUMaster(string uid = null)
    {
        var mockSKUMaster = new Mock<ISKUMaster>();
        mockSKUMaster.Setup(x => x.SKU).Returns(CreateMockSKU(uid));
        mockSKUMaster.Setup(x => x.SKUAttributes).Returns(CreateMockSKUAttributes());
        mockSKUMaster.Setup(x => x.SKUUOMs).Returns(CreateMockSKUUOMs());
        mockSKUMaster.Setup(x => x.ApplicableTaxUIDs).Returns(new List<string> { Guid.NewGuid().ToString() });
        mockSKUMaster.Setup(x => x.SKUConfigs).Returns(new List<ISKUConfig>());
        mockSKUMaster.Setup(x => x.CustomSKUFields).Returns(new List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>());
        mockSKUMaster.Setup(x => x.FileSysList).Returns(new List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>());
        return mockSKUMaster.Object;
    }

    /// <summary>
    /// Creates a list of mock SKU attributes.
    /// </summary>
    /// <returns>List of mock ISKUAttributes</returns>
    public static List<ISKUAttributes> CreateMockSKUAttributes()
    {
        var mockAttribute = new Mock<ISKUAttributes>();
        mockAttribute.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        mockAttribute.Setup(x => x.SKUUID).Returns(Guid.NewGuid().ToString());
        mockAttribute.Setup(x => x.Type).Returns("Color");
        mockAttribute.Setup(x => x.Code).Returns("CLR");
        mockAttribute.Setup(x => x.Value).Returns("Blue");
        mockAttribute.Setup(x => x.ParentType).Returns("Appearance");
        mockAttribute.Setup(x => x.ActionType).Returns(ActionType.Add);
        
        return new List<ISKUAttributes> { mockAttribute.Object };
    }

    /// <summary>
    /// Creates a list of mock SKU UOMs.
    /// </summary>
    /// <returns>List of mock ISKUUOM</returns>
    public static List<ISKUUOM> CreateMockSKUUOMs()
    {
        var mockUOM = new Mock<ISKUUOM>();
        mockUOM.Setup(x => x.UID).Returns(Guid.NewGuid().ToString());
        mockUOM.Setup(x => x.SKUUID).Returns(Guid.NewGuid().ToString());
        mockUOM.Setup(x => x.Code).Returns("PCS");
        mockUOM.Setup(x => x.Name).Returns("Pieces");
        mockUOM.Setup(x => x.IsBaseUOM).Returns(true);
        mockUOM.Setup(x => x.Multiplier).Returns(1);
        
        return new List<ISKUUOM> { mockUOM.Object };
    }

    #endregion

    #region Request Objects

    /// <summary>
    /// Creates a valid paging request for testing.
    /// </summary>
    /// <param name="pageNumber">Page number. Default is 1.</param>
    /// <param name="pageSize">Page size. Default is 10.</param>
    /// <param name="isCountRequired">Whether count is required. Default is true.</param>
    /// <returns>Valid PagingRequest object</returns>
    public static PagingRequest CreateValidPagingRequest(int pageNumber = 1, int pageSize = 10, bool isCountRequired = true)
    {
        return new PagingRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsCountRequired = isCountRequired,
            FilterCriterias = new List<FilterCriteria>(),
            SortCriterias = new List<SortCriteria>
            {
                new SortCriteria("Name", SortDirection.Asc)
            }
        };
    }

    /// <summary>
    /// Creates a paging request with filter criteria.
    /// </summary>
    /// <param name="filterName">Filter field name</param>
    /// <param name="filterValue">Filter value</param>
    /// <param name="filterType">Filter type. Default is Equal.</param>
    /// <returns>PagingRequest with filter criteria</returns>
    public static PagingRequest CreatePagingRequestWithFilter(string filterName, object filterValue, FilterType filterType = FilterType.Equal)
    {
        var pagingRequest = CreateValidPagingRequest();
        pagingRequest.FilterCriterias.Add(new FilterCriteria(filterName, filterValue, filterType));
        return pagingRequest;
    }

    /// <summary>
    /// Creates an invalid paging request for testing error scenarios.
    /// </summary>
    /// <returns>Invalid PagingRequest object</returns>
    public static PagingRequest CreateInvalidPagingRequest()
    {
        return new PagingRequest
        {
            PageNumber = -1,
            PageSize = -1,
            IsCountRequired = false
        };
    }

    /// <summary>
    /// Creates a PrepareSKURequestModel for testing.
    /// </summary>
    /// <param name="skuUIDs">List of SKU UIDs. If null, creates a single test UID.</param>
    /// <returns>PrepareSKURequestModel object</returns>
    public static PrepareSKURequestModel CreatePrepareSKURequestModel(List<string> skuUIDs = null)
    {
        return new PrepareSKURequestModel
        {
            SKUUIDs = skuUIDs ?? new List<string> { Guid.NewGuid().ToString() }
        };
    }

    #endregion

    #region Response Objects

    /// <summary>
    /// Creates a paged response for testing.
    /// </summary>
    /// <typeparam name="T">Type of data in the response</typeparam>
    /// <param name="data">Data to include in the response</param>
    /// <param name="totalCount">Total count of items</param>
    /// <returns>PagedResponse object</returns>
    public static PagedResponse<T> CreatePagedResponse<T>(IEnumerable<T> data, int totalCount)
    {
        return new PagedResponse<T>
        {
            PagedData = data,
            TotalCount = totalCount
        };
    }

    #endregion

    #region Validation Helpers

    /// <summary>
    /// Validates that a SKU object has required properties set.
    /// </summary>
    /// <param name="sku">SKU object to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidSKU(ISKU sku)
    {
        return sku != null &&
               !string.IsNullOrEmpty(sku.UID) &&
               !string.IsNullOrEmpty(sku.Code) &&
               !string.IsNullOrEmpty(sku.Name) &&
               !string.IsNullOrEmpty(sku.CompanyUID) &&
               !string.IsNullOrEmpty(sku.OrgUID);
    }

    /// <summary>
    /// Validates that a SKUV1 object has required properties set.
    /// </summary>
    /// <param name="skuV1">SKUV1 object to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidSKUV1(ISKUV1 skuV1)
    {
        return IsValidSKU(skuV1) &&
               !string.IsNullOrEmpty(skuV1.HSNCode) &&
               skuV1.ProductCategoryId > 0;
    }

    #endregion
} 