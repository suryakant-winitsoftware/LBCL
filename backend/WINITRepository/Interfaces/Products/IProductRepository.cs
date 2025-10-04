using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Proucts.PostgreSQLProductsRepository;

namespace WINITRepository.Interfaces.Products
{
    public interface IProductRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsAll();
      
        Task<WINITSharedObjects.Models.Product> GetProductByProductCode(string productCode);
        Task<WINITSharedObjects.Models.Product> CreateProduct(WINITSharedObjects.Models.Product Product);
    
       // Task<int> UpdateProduct(string product_code, WINITSharedObjects.Models.Product UpdateProduct);
        Task<int> UpdateProduct( WINITSharedObjects.Models.Product UpdateProduct);
     
        Task<int> DeleteProduct(string productCode);
        Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsFiltered(string product_code, String product_name, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsPaged(int pageNumber, int pageSize);
       
        Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsSorted(List<SortCriteria> sortCriterias);

        //ProductConfig All EndPoints

        Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigAll();

        Task<WINITSharedObjects.Models.ProductConfig> GetProductConfigByskuConfigId(int skuConfigId);
        Task<WINITSharedObjects.Models.ProductConfig> CreateProductConfig(WINITSharedObjects.Models.ProductConfig CreateProductConfig);

        Task<int> UpdateProductConfig( WINITSharedObjects.Models.ProductConfig UpdateProductConfig);

        Task<int> DeleteProductConfig(int skuConfigId);
        Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize);

        Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias);

        //ProductUOM All EndPoints

        Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMAll();

        Task<WINITSharedObjects.Models.ProductUOM> GetProductUOMByProductUomId(int productUomId);
        Task<WINITSharedObjects.Models.ProductUOM> CreateProductUOM(WINITSharedObjects.Models.ProductUOM CreateProductUOM);

        Task<int> UpdateProductUOM( ProductUOM UpdateProductUOM);

        Task<int> DeleteProductUOM(int productUomId);
        Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize);

        Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);
        //GetProductMaster
        Task<IEnumerable<WINITSharedObjects.Models.ProductAttributes>> GetProductAttributes();
        Task<WINITSharedObjects.Models.ProductDimensionBridge> CreateProductDimensionBridge(WINITSharedObjects.Models.ProductDimensionBridge CreateProductDimensionBridge);
        Task<int> DeleteProductDimensionBridge(int product_dimension_bridge_id);
        Task<WINITSharedObjects.Models.ProductTypeBridge> CreateProductTypeBridge(WINITSharedObjects.Models.ProductTypeBridge CreateProductTypeBridge);
        Task<int> DeleteProductTypeBridge(int product_type_bridge_id);
        //ProductType
        Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeAll();
        Task<WINITSharedObjects.Models.ProductType> GetProductTypeByProductTypeId( int productTypeId);
        Task<WINITSharedObjects.Models.ProductType> CreateProductType(WINITSharedObjects.Models.ProductType CreateProductType);
        Task<int> UpdateProductType( ProductType UpdateProductType);
        Task<int> DeleteProductType(int productTypeId);
        Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypePaged(int pageNumber, int pageSize);
        Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        //ProductDimension
        Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionAll();
        Task<WINITSharedObjects.Models.ProductDimension> GetProductDimensionByProductDimensionId(int productDimensionId);
        Task<WINITSharedObjects.Models.ProductDimension> CreateProductDimensionList(WINITSharedObjects.Models.ProductDimension CreateProductDimension);
        Task<int> UpdateProductDimension( ProductDimension UpdateProductDimension);
        Task<int> DeleteProductDimension(int productDimensionId);
        Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime);
        Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize);
        Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        //singleendpoint for all methods
        Task<IEnumerable<WINITSharedObjects.Models.Product>> AllGetProduct(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
            int pageSize, List<FilterCriteria> filterCriterias);

    }
}
