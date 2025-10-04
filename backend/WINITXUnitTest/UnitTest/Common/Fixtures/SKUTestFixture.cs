using Winit.Modules.SKU.Model.Classes;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Shared.Models.Enums;

namespace WINITXUnitTest.UnitTest.Common.Fixtures;

public static class SKUTestFixture
{
    public static ISKU CreateSampleSKU(string uid = "TEST-SKU-001")
    {
        return new SKU
        {
            UID = uid,
            CompanyUID = "COMP-001",
            OrgUID = "ORG-001",
            Code = "SKU-001",
            Name = "Test SKU",
            ArabicName = "اختبار SKU",
            AliasName = "Test SKU Alias",
            LongName = "Test SKU Long Name",
            BaseUOM = "PCS",
            OuterUOM = "BOX",
            FromDate = DateTime.Now,
            ToDate = DateTime.Now.AddYears(1),
            IsStockable = true,
            ParentUID = null,
            IsActive = true,
            IsThirdParty = false,
            SupplierOrgUID = "SUPP-001",
            SKUImage = "test-sku.jpg",
            CatalogueURL = "http://example.com/test-sku",
            Qty = 100,
            ActionType = ActionType.Add
        };
    }

    public static ISKUV1 CreateSampleSKUV1(string uid = "TEST-SKU-001")
    {
        return new SKUV1
        {
            UID = uid,
            CompanyUID = "COMP-001",
            OrgUID = "ORG-001",
            Code = "SKU-001",
            Name = "Test SKU",
            ArabicName = "اختبار SKU",
            AliasName = "Test SKU Alias",
            LongName = "Test SKU Long Name",
            BaseUOM = "PCS",
            OuterUOM = "BOX",
            FromDate = DateTime.Now,
            ToDate = DateTime.Now.AddYears(1),
            IsStockable = true,
            ParentUID = null,
            IsActive = true,
            IsThirdParty = false,
            SupplierOrgUID = "SUPP-001",
            SKUImage = "test-sku.jpg",
            CatalogueURL = "http://example.com/test-sku",
            Qty = 100,
            ActionType = ActionType.Add,
            ModelCode = "MODEL-001",
            Year = 2024,
            Type = "Product",
            ProductType = "Standard",
            Category = "Test Category",
            Tonnage = "1.5",
            Capacity = "1000",
            StarRating = "4",
            ProductCategoryId = 1,
            ProductCategoryName = "Test Category",
            ItemSeries = "SERIES-001",
            HSNCode = "HSN001",
            IsAvailableInApMaster = true,
            FilterKeys = new HashSet<string> { "Test", "Category" }
        };
    }

    public static List<ISKU> CreateSampleSKUList(int count = 10)
    {
        var skuList = new List<ISKU>();
        for (int i = 1; i <= count; i++)
        {
            skuList.Add(CreateSampleSKU($"TEST-SKU-{i:000}"));
        }
        return skuList;
    }

    public static ISKUAttributes CreateSampleSKUAttribute(string skuUid = "TEST-SKU-001")
    {
        return new SKUAttributes
        {
            UID = $"ATTR-{skuUid}",
            SKUUID = skuUid,
            Type = "BRAND",
            Code = "BRAND-001",
            Value = "Test Brand",
            ParentType = null,
            ActionType = ActionType.Add
        };
    }

    public static ISKUUOM CreateSampleSKUUOM(string skuUid = "TEST-SKU-001")
    {
        return new SKUUOM
        {
            UID = $"UOM-{skuUid}",
            SKUUID = skuUid,
            Code = "PCS",
            Name = "Pieces",
            Label = "PCS",
            Barcodes = "123456789",
            IsBaseUOM = true,
            IsOuterUOM = false,
            Multiplier = 1,
            Length = 10,
            Width = 5,
            Height = 2,
            Depth = 5,
            Volume = 100,
            Weight = 0.5m,
            GrossWeight = 0.6m,
            DimensionUnit = "CM",
            VolumeUnit = "CM3",
            WeightUnit = "KG",
            GrossWeightUnit = "KG",
            Liter = 0.1m,
            KGM = 0.5m
        };
    }

    public static ISKUConfig CreateSampleSKUConfig(string skuUid = "TEST-SKU-001")
    {
        return new SKUConfig
        {
            UID = $"CONFIG-{skuUid}",
            OrgUID = "ORG-001",
            Name = "Default Config",
            DistributionChannelOrgUID = "DIST-001",
            SKUUID = skuUid,
            CanBuy = true,
            CanSell = true,
            BuyingUOM = "PCS",
            SellingUOM = "PCS",
            IsActive = true
        };
    }

    public static ISKUMaster CreateSampleSKUMaster(string skuUid = "TEST-SKU-001")
    {
        return new SKUMaster
        {
            SKU = CreateSampleSKU(skuUid),
            SKUAttributes = new List<ISKUAttributes> { CreateSampleSKUAttribute(skuUid) },
            SKUUOMs = new List<ISKUUOM> { CreateSampleSKUUOM(skuUid) },
            SKUConfigs = new List<ISKUConfig> { CreateSampleSKUConfig(skuUid) },
            ApplicableTaxUIDs = new List<string> { "TAX-001" },
            CustomSKUFields = new List<Winit.Modules.CustomSKUField.Model.Interfaces.ICustomSKUFields>(),
            FileSysList = new List<Winit.Modules.FileSys.Model.Interfaces.IFileSys>()
        };
    }
} 