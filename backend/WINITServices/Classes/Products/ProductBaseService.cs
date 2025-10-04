using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Proucts.PostgreSQLProductsRepository;
 
namespace WINITServices.Classes.Products
{
    public abstract class ProductBaseService : Interfaces.IProductService
    {
        protected readonly WINITRepository.Interfaces.Products.IProductRepository _productRepository;
        public ProductBaseService(WINITRepository.Interfaces.Products.IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public abstract Task<int> AddCustomer();
        
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsAll();
    
        public abstract Task<WINITSharedObjects.Models.Product> GetProductByProductCode(string productCode);

        public abstract Task<WINITSharedObjects.Models.Product> CreateProduct(WINITSharedObjects.Models.Product Product);
     
       // public abstract Task<int> UpdateProduct( string product_code, WINITSharedObjects.Models.Product UpdateProduct);
        public abstract Task<int> UpdateProduct(  WINITSharedObjects.Models.Product UpdateProduct);
    
        public abstract Task<int> DeleteProduct(string productCode);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsFiltered(string product_code, String product_name, DateTime CreatedTime, DateTime ModifiedTime);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsPaged(int pageNumber, int pageSize);
      
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsSorted(List<SortCriteria> sortCriterias);


        //ProductConfig All EndPoints

        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigAll();

        public abstract Task<WINITSharedObjects.Models.ProductConfig> GetProductConfigByskuConfigId(int skuConfigId);
        public abstract Task<WINITSharedObjects.Models.ProductConfig> CreateProductConfig(WINITSharedObjects.Models.ProductConfig CreateProductConfig);

        public abstract Task<int> UpdateProductConfig( WINITSharedObjects.Models.ProductConfig UpdateProductConfig);

        public abstract Task<int> DeleteProductConfig(int skuConfigId);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize);

        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias);

        //ProductUOM All EndPoints

        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMAll();

        public abstract Task<WINITSharedObjects.Models.ProductUOM> GetProductUOMByProductUomId(int productUomId);
        public abstract Task<WINITSharedObjects.Models.ProductUOM> CreateProductUOM(WINITSharedObjects.Models.ProductUOM CreateProductUOM);

        public abstract Task<int> UpdateProductUOM( ProductUOM UpdateProductUOM);

        public abstract Task<int> DeleteProductUOM(int productUomId);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize);

        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductAttributes>> GetProductAttributes();

        public abstract Task<int> DeleteProductDimensionBridge(int product_dimension_bridge_id);
        public abstract Task<WINITSharedObjects.Models.ProductDimensionBridge> CreateProductDimensionBridge(WINITSharedObjects.Models.ProductDimensionBridge CreateProductDimensionBridge);



        public abstract Task<WINITSharedObjects.Models.ProductTypeBridge> CreateProductTypeBridge(WINITSharedObjects.Models.ProductTypeBridge CreateProductTypeBridge);
        public abstract Task<int> DeleteProductTypeBridge(int product_type_bridge_id);

        //ProductType
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeAll();
        public abstract Task<WINITSharedObjects.Models.ProductType> GetProductTypeByProductTypeId(int productTypeId);
        public abstract Task<WINITSharedObjects.Models.ProductType> CreateProductType(WINITSharedObjects.Models.ProductType CreateProductType);
        public abstract Task<int> UpdateProductType( ProductType UpdateProductType);
        public abstract Task<int> DeleteProductType(int productTypeId);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypePaged(int pageNumber, int pageSize);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        //ProductDimension
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionAll();
        public abstract Task<WINITSharedObjects.Models.ProductDimension> GetProductDimensionByProductDimensionId(int productDimensionId);
        public abstract Task<WINITSharedObjects.Models.ProductDimension> CreateProductDimensionList(WINITSharedObjects.Models.ProductDimension CreateProductDimension);
        public abstract Task<int> UpdateProductDimension( ProductDimension UpdateProductDimension);
        public abstract Task<int> DeleteProductDimension(int productDimensionId);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);

        public abstract Task<IEnumerable<WINITSharedObjects.Models.Product>> AllGetProduct(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias);
    }
}
