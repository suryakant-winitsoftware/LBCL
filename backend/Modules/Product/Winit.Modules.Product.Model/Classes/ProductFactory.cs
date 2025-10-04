using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Product.Model.Interfaces;


namespace Winit.Modules.Product.Model.Classes
{
    public class ProductFactory : IFactory
    {
        private readonly string _className;

        public ProductFactory(string className)
        {
            _className = className;
        }

        public object? CreateInstance()
        {
            switch (_className)
            {
                case ProductModule.Product:
                    return new Product();
                case ProductModule.ProductAttributes:
                    return new ProductAttributes();
                case ProductModule.ProductTypeBridge:
                    return new ProductTypeBridge();
                case ProductModule.ProductConfig:
                    return new ProductConfig();
                case ProductModule.ProductType:
                    return new ProductType();
                case ProductModule.ProductDimensionBridge:
                    return new ProductDimensionBridge();
                case ProductModule.ProductUOM:
                    return new ProductUOM();
                case ProductModule.ProductDimension:
                    return new ProductDimension();
                default:
                    return null;
            }
        }
    }
}
