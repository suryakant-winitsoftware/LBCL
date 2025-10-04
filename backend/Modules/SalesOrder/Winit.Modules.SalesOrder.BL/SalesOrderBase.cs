using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.SalesOrder.BL.UIInterfaces;

namespace Winit.Modules.SalesOrder.BL
{
    public class SalesOrderBase
    {
        protected ISalesOrderViewModel _viewModel;
        private bool _isViewModelSet = false;
        private void Validate()
        {
            if (_viewModel == null)
            {
                throw new Exception("salesOrderViewModel can't be null");
            }
            else
            {
                UpdateInitialData();
            }
        }
        /// <summary>
        /// Must be called to initialize SalesOrderViewModel
        /// </summary>
        /// <param name="salesOrderViewModel"></param>
        protected void Initialize(ISalesOrderViewModel salesOrderViewModel)
        {
            _viewModel = salesOrderViewModel;
            _isViewModelSet = true;
            Validate();
        }
        /// <summary>
        /// All public method of child class should call this method before doing any operation
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected void CheckIfViewModelSet()
        {
            if (!_isViewModelSet)
            {
                throw new InvalidOperationException("SetSalesOrderViewModel must be called before calling any method.");
            }
        }
        /// <summary>
        /// Override when you need to do any initial data update while creating object
        /// </summary>
        protected virtual void UpdateInitialData()
        {

        }
        // Any common method related to all the sales order we will put here
        
    }
}
