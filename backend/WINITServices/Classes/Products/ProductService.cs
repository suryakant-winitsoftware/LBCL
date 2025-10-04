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
    public class ProductService : ProductBaseService
    {
        public ProductService(WINITRepository.Interfaces.Products.IProductRepository productRepository) : base(productRepository)
        {

        }
        public async override Task<int> AddCustomer()
        {
            throw new NotImplementedException();
        }


        public async override Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsAll()
        {
            return await _productRepository.GetProductsAll();
        }

        public async override Task<WINITSharedObjects.Models.Product> GetProductByProductCode(string productCode)
        {
            return await _productRepository.GetProductByProductCode(productCode);
        }
        public async override Task<WINITSharedObjects.Models.Product> CreateProduct(WINITSharedObjects.Models.Product CreateProduct)
        {
            return await _productRepository.CreateProduct(CreateProduct);

        }


        //public async override Task<int> UpdateProduct(string product_code, WINITSharedObjects.Models.Product UpdateProduct)
        //{
        //    return await _productRepository.UpdateProduct(product_code, UpdateProduct);
        //}

        public async override Task<int> UpdateProduct( WINITSharedObjects.Models.Product UpdateProduct)
        {
            return await _productRepository.UpdateProduct(UpdateProduct);
        }
        public async override Task<int> DeleteProduct(string productCode)
        {
            return await _productRepository.DeleteProduct(productCode);
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsFiltered(string product_code, String product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productRepository.GetProductsFiltered(product_code, product_name, CreatedTime, ModifiedTime);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsPaged(int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductsPaged(pageNumber, pageSize);
        }

        //public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(string sortField, SortDirection sortDirection)
        //{
        //    return await _customerRepository.GetCustomersSorted(sortField, sortDirection);
        //}
        public async override Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsSorted(List<SortCriteria> sortCriterias)
        {
            return await _productRepository.GetProductsSorted(sortCriterias);
        }

        //ProductConfig All EndPoints


        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigAll()
        {
            return await _productRepository.GetProductConfigAll();
        }
        public async override Task<WINITSharedObjects.Models.ProductConfig> GetProductConfigByskuConfigId(int skuConfigId)
        {
            return await _productRepository.GetProductConfigByskuConfigId(skuConfigId);
        }
        public async override Task<WINITSharedObjects.Models.ProductConfig> CreateProductConfig(WINITSharedObjects.Models.ProductConfig CreateProductConfig)
        {
            return await _productRepository.CreateProductConfig(CreateProductConfig);
        }

        public async override Task<int> UpdateProductConfig( WINITSharedObjects.Models.ProductConfig UpdateProductConfig)
        {
            return await _productRepository.UpdateProductConfig(UpdateProductConfig);
    }

        public async override Task<int> DeleteProductConfig(int skuConfigId)
        {
            return await _productRepository.DeleteProductConfig(skuConfigId);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productRepository.GetProductConfigFiltered(ProductCode, CreatedTime, ModifiedTime);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductConfigPaged(pageNumber, pageSize);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
        {
            return await _productRepository.GetProductConfigSorted(sortCriterias);
        }

        //ProductUOM All EndPoints



        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMAll()
        {
            return await _productRepository.GetProductUOMAll();
        }
        public async override Task<WINITSharedObjects.Models.ProductUOM> GetProductUOMByProductUomId(int productUomId)
        {
            return await _productRepository.GetProductUOMByProductUomId(productUomId);
        }
        public async override Task<WINITSharedObjects.Models.ProductUOM> CreateProductUOM(WINITSharedObjects.Models.ProductUOM CreateProductUOM)
        {
            return await _productRepository.CreateProductUOM(CreateProductUOM);
        }

        public async override Task<int> UpdateProductUOM( ProductUOM UpdateProductUOM)
        {
            return await _productRepository.UpdateProductUOM( UpdateProductUOM);
        }

        public async override Task<int> DeleteProductUOM(int productUomId)
        {
            return await _productRepository.DeleteProductUOM(productUomId);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productRepository.GetProductUOMFiltered(ProductCode, CreatedTime, ModifiedTime);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductUOMPaged(pageNumber, pageSize);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductUOM>> GetProductUOMSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            return await _productRepository.GetProductUOMSorted(sortCriterias);
        }


        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductAttributes>> GetProductAttributes()
        {
            return await _productRepository.GetProductAttributes();
        }

        public async override Task<WINITSharedObjects.Models.ProductDimensionBridge> CreateProductDimensionBridge(WINITSharedObjects.Models.ProductDimensionBridge CreateProductDimensionBridge)
        {
            return await _productRepository.CreateProductDimensionBridge(CreateProductDimensionBridge);
        }

        public async override Task<int> DeleteProductDimensionBridge(int product_dimension_bridge_id)
        {
            return await _productRepository.DeleteProductDimensionBridge(product_dimension_bridge_id);
        }

        public async override Task<WINITSharedObjects.Models.ProductTypeBridge> CreateProductTypeBridge(WINITSharedObjects.Models.ProductTypeBridge CreateProductTypeBridge)
        {
            return await _productRepository.CreateProductTypeBridge(CreateProductTypeBridge);
        }

        public async override Task<int> DeleteProductTypeBridge(int product_type_bridge_id)
        {
            return await _productRepository.DeleteProductTypeBridge(product_type_bridge_id);
        }


        //productype

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeAll()
        {
            return await _productRepository.GetProductTypeAll();
        }
        public async override Task<WINITSharedObjects.Models.ProductType> GetProductTypeByProductTypeId(int productTypeId)
        {
            return await _productRepository.GetProductTypeByProductTypeId(productTypeId);
        }
        public async override Task<WINITSharedObjects.Models.ProductType> CreateProductType(WINITSharedObjects.Models.ProductType CreateProductType)
        {
            return await _productRepository.CreateProductType(CreateProductType);
        }

        public async override Task<int> UpdateProductType( ProductType UpdateProductType)
        {
            return await _productRepository.UpdateProductType( UpdateProductType);
        }

        public async override Task<int> DeleteProductType(int productTypeId)
        {
            return await _productRepository.DeleteProductType(productTypeId);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productRepository.GetProductTypeFiltered(product_type_code, CreatedTime, ModifiedTime);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypePaged(int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductTypePaged(pageNumber, pageSize);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductType>> GetProductTypeSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            return await _productRepository.GetProductTypeSorted(sortCriterias);
        }

        //ProductDimension


        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionAll()
        {
            return await _productRepository.GetProductDimensionAll();
        }
        public async override Task<WINITSharedObjects.Models.ProductDimension> GetProductDimensionByProductDimensionId(int productDimensionId)
        {
            return await _productRepository.GetProductDimensionByProductDimensionId(productDimensionId);
        }
        public async override Task<WINITSharedObjects.Models.ProductDimension> CreateProductDimensionList(WINITSharedObjects.Models.ProductDimension CreateProductDimension)
        {
            return await _productRepository.CreateProductDimensionList(CreateProductDimension);
        }

        public async override Task<int> UpdateProductDimension( ProductDimension UpdateProductDimension)
        {
            return await _productRepository.UpdateProductDimension( UpdateProductDimension);
        }

        public async override Task<int> DeleteProductDimension(int productDimensionId)
        {
            return await _productRepository.DeleteProductDimension(productDimensionId);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            return await _productRepository.GetProductDimensionFiltered(product_dimension_code, CreatedTime, ModifiedTime);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductDimensionPaged(pageNumber, pageSize);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.ProductDimension>> GetProductDimensionSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            return await _productRepository.GetProductDimensionSorted(sortCriterias);
        }


        public async override Task<IEnumerable<WINITSharedObjects.Models.Product>> AllGetProduct(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias, int pageNumber,
           int pageSize, List<FilterCriteria> filterCriterias)
        {
            return await _productRepository.AllGetProduct(sortCriterias, pageNumber , pageSize, filterCriterias);
        }
    }

}
