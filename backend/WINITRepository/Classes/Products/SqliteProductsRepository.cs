using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Models;
using WINITSharedObjects.Enums;

namespace WINITRepository.Classes.Proucts
{
    public class SqliteProductsRepository: Interfaces.Products.IProductRepository
    {
        private readonly string _connectionString;
        public SqliteProductsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SQLite");
        }

       

        public async Task<IEnumerable<WINITSharedObjects.Models.Product>> GetProductsAll()
        {
            IEnumerable < WINITSharedObjects.Models.Product > customerList = new List<WINITSharedObjects.Models.Product>() {
                //new WINITSharedObjects.Models.Customer{CustomerId = 1, UID = "1", CustomerCode ="C0001", CustomerName = "Customer1" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 2, UID = "2", CustomerCode ="C0002", CustomerName = "Customer2" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 3, UID = "3", CustomerCode ="C0003", CustomerName = "Customer3" }
                    };
            return await Task.FromResult(customerList);
        }
        public async Task<WINITSharedObjects.Models.Product> GetProductByProductCode(string productCode)
        {
            WINITSharedObjects.Models.Product Products = new WINITSharedObjects.Models.Product(); 
           //{ CustomerId = 1, UID = "1", CustomerCode = "C0001", CustomerName = "Customer1" };
            return await Task.FromResult(Products);
        }
        public Task<Product> CreateProduct(Product Product)
        {
            throw new NotImplementedException();
        }
        //public Task<int> UpdateCustomer(Int64 CustomerId, updateCustomer updateCustomer)
        //{
        //    throw new NotImplementedException();
        //}
        public Task<int> UpdateProduct( Product UpdateProduct)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProduct(string productCode)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProductsFiltered(string product_code, String product_name, DateTime CreatedTime, DateTime ModifiedTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> GetProductsPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        //public Task<IEnumerable<Customer>> GetCustomersSorted(string sortField, SortDirection sortDirection)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<IEnumerable<Product>> GetProductsSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }
        //ProductConfig All EndPoints
        public Task<IEnumerable<ProductConfig>> GetProductConfigAll()
        {
            throw new NotImplementedException();
        }

        public Task<ProductConfig> GetProductConfigByskuConfigId(int skuConfigId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductConfig> CreateProductConfig(ProductConfig CreateProductConfig)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateProductConfig( ProductConfig UpdateProductConfig)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductConfig(int skuConfigId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductConfig>> GetProductConfigFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductConfig>> GetProductConfigPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductConfig>> GetProductConfigSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }
        //ProductUOM All EndPoints

        public Task<IEnumerable<ProductUOM>> GetProductUOMAll()
        {
            throw new NotImplementedException();
        }

        public Task<ProductUOM> GetProductUOMByProductUomId(int productUomId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductUOM> CreateProductUOM(ProductUOM CreateProductUOM)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateProductUOM(ProductUOM UpdateProductUOM)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductUOM(int productUomId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductUOM>> GetProductUOMFiltered(string ProductCode, DateTime CreatedTime, DateTime ModifiedTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductUOM>> GetProductUOMPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductUOM>> GetProductUOMSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductAttributes>> GetProductAttributes()
        {
            throw new NotImplementedException();
        }

        public Task<ProductDimensionBridge> CreateProductDimensionBridge(ProductDimensionBridge CreateProductDimensionBridge)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductDimensionBridge(int product_dimension_bridge_id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductTypeBridge> CreateProductTypeBridge(ProductTypeBridge CreateProductTypeBridge)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductTypeBridge(int product_type_bridge_id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductType>> GetProductTypeAll()
        {
            throw new NotImplementedException();
        }

        public Task<ProductType> GetProductTypeByProductTypeId(int productTypeId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductType> CreateProductType(ProductType CreateProductType)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateProductType( ProductType UpdateProductType)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductType(int productTypeId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductType>> GetProductTypeFiltered(string product_type_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductType>> GetProductTypePaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductType>> GetProductTypeSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductDimension>> GetProductDimensionAll()
        {
            throw new NotImplementedException();
        }

        public Task<ProductDimension> GetProductDimensionByProductDimensionId(int productDimensionId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDimension> CreateProductDimensionList(ProductDimension CreateProductDimension)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateProductDimension( ProductDimension UpdateProductDimension)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductDimension(int productDimensionId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductDimension>> GetProductDimensionFiltered(string product_dimension_code, DateTime CreatedTime, DateTime ModifiedTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductDimension>> GetProductDimensionPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductDimension>> GetProductDimensionSorted(List<SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Product>> AllGetProduct(List<SortCriteria> sortCriterias, int pageNumber, int pageSize, List<FilterCriteria> filterCriterias)
        {
            throw new NotImplementedException();
        }
    }
}
