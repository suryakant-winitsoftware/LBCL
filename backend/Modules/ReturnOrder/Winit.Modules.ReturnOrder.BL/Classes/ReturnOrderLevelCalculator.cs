using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ReturnOrder.BL.Interfaces;
using Winit.Shared.CommonUtilities.Common;

namespace Winit.Modules.ReturnOrder.BL.Classes
{
    internal class ReturnOrderLevelCalculator:IReturnOrderLevelCalculator
    {
        List<Model.Interfaces.IReturnOrderItemView> _returnOrderItemViews;

        public IReturnOrderViewModel _returnOrderViewModel { get; set; }

        public ReturnOrderLevelCalculator(IReturnOrderViewModel returnOrderViewModel)
        {
            _returnOrderViewModel = returnOrderViewModel;
            _returnOrderItemViews = _returnOrderViewModel.ReturnOrderItemViews.Where(e => e.IsSelected && e.OrderQty > 0).ToList();
        }
        public async Task ComputeOrderLevelTaxesAndOrderSummary()
        {
            UpdateHeader();
            // If Taxable customer then only calculate Tax
            if (_returnOrderViewModel.SelectedStoreViewModel.IsTaxApplicable)
            {
                CalculateTaxForInvoice();
                CalculateTaxForInvoiceTaxOnTax();
            }
            UpdateLineWiseProrataTax();
            await Task.Delay(1);
        }
        private void UpdateHeaderAmount()
        {
            _returnOrderViewModel.TotalAmount = CommonFunctions.RoundForSystem(_returnOrderItemViews.Sum(e => e.TotalAmount));
            _returnOrderViewModel.TotalLineDiscount = CommonFunctions.RoundForSystem(_returnOrderItemViews.Sum(e => e.TotalDiscount));
            _returnOrderViewModel.TotalCashDiscount = 0;
            _returnOrderViewModel.TotalHeaderDiscount = 0;
            _returnOrderViewModel.TotalLineTax = CommonFunctions.RoundForSystem(_returnOrderItemViews.Sum(e => e.TotalTax));
            _returnOrderViewModel.TotalHeaderTax = 0;
        }
        private void UpdateHeaderCount()
        {
            _returnOrderViewModel.LineCount = _returnOrderItemViews.Count(); ;
            _returnOrderViewModel.QtyCount = _returnOrderItemViews.Sum(e => e.ReturnedQty);
        }
        private void UpdateHeader()
        {
            UpdateHeaderAmount();
            UpdateHeaderCount();
        }
        private void CalculateTaxForInvoice()
        {
            _returnOrderViewModel.TotalHeaderTax = (_returnOrderViewModel.TotalAmount - _returnOrderViewModel.TotalDiscount) * .01m;
            //salesOrderViewModel.AppliedOrderWiseTaxViewList.Clear();
            //salesOrderViewModel.ProrataTaxAmount = 0;
            //List<string> calculationTypeList = salesOrderViewModel.OrderWiseTaxViewList
            //    .Where(e => e.ApplicableAt == WinIT.mSFA.Shared.sFAModel.TaxApplicableType.INVOICE)
            //    .Select(e => e.TaxCalculationType).Distinct().ToList<string>();
            //foreach (string calculationType in calculationTypeList)
            //{
            //    if (calculationType == WinIT.mSFA.Shared.sFAModel.TaxCalculationType.PERCENTAGE)
            //    {
            //        List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> percentageOrderWiseTaxViewList = salesOrderViewModel.OrderWiseTaxViewList
            //                .Where(e => e.TaxCalculationType == WinIT.mSFA.Shared.sFAModel.TaxCalculationType.PERCENTAGE
            //                && e.ApplicableAt == WinIT.mSFA.Shared.sFAModel.TaxApplicableType.INVOICE).ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();
            //        if (percentageOrderWiseTaxViewList != null && percentageOrderWiseTaxViewList.Count() > 0)
            //        {
            //            foreach (Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxView in percentageOrderWiseTaxViewList)
            //            {
            //                Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxViewClone = new Shared.sFAModel.Domain.Tax.OrderWiseTaxView(orderWiseTaxView);
            //                if (salesOrderViewModel.IsPriceInclusiveVat)
            //                {
            //                    orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem(((salesOrderViewModel.TotalAmount - salesOrderViewModel.TotalDiscount)
            //                    * orderWiseTaxViewClone.BaseTaxRate) / (100 + orderWiseTaxViewClone.BaseTaxRate));
            //                }
            //                else
            //                {
            //                    orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem((salesOrderViewModel.TotalAmount - salesOrderViewModel.TotalDiscount)
            //                    * orderWiseTaxViewClone.BaseTaxRate * Shared.Util.CommonFunction.GetDecimalValue(.01));
            //                }
            //                salesOrderViewModel.ProrataTaxAmount += orderWiseTaxViewClone.TaxAmount;
            //                salesOrderViewModel.AppliedOrderWiseTaxViewList.Add(orderWiseTaxViewClone);
            //            }
            //        }

            //    }
            //    else if (calculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE_SLAB)
            //    {
            //        List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> percentageOrderWiseTaxViewList = salesOrderViewModel.OrderWiseTaxViewList
            //                .Where(e => e.TaxCalculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE_SLAB
            //                && e.ApplicableAt == Shared.sFAModel.TaxApplicableType.INVOICE
            //                && salesOrderViewModel.TotalAmount >= e.RangeStart && salesOrderViewModel.TotalAmount < e.RangeEnd)
            //                .ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();
            //        if (percentageOrderWiseTaxViewList != null && percentageOrderWiseTaxViewList.Count() > 0)
            //        {
            //            foreach (Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxView in percentageOrderWiseTaxViewList)
            //            {
            //                Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxViewClone = new Shared.sFAModel.Domain.Tax.OrderWiseTaxView(orderWiseTaxView);
            //                if (salesOrderViewModel.IsPriceInclusiveVat)
            //                {
            //                    orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem(((salesOrderViewModel.TotalAmount - salesOrderViewModel.TotalDiscount)
            //                        * orderWiseTaxViewClone.TaxRate) / (100 + orderWiseTaxViewClone.TaxRate));
            //                }
            //                else
            //                {
            //                    orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem((salesOrderViewModel.TotalAmount - salesOrderViewModel.TotalDiscount)
            //                    * orderWiseTaxViewClone.TaxRate * Shared.Util.CommonFunction.GetDecimalValue(.01));
            //                }

            //                salesOrderViewModel.ProrataTaxAmount += orderWiseTaxViewClone.TaxAmount;
            //                salesOrderViewModel.AppliedOrderWiseTaxViewList.Add(orderWiseTaxViewClone);
            //            }
            //        }
            //    }
            //}
        }
        private void CalculateTaxForInvoiceTaxOnTax()
        {
            //List<string> calculationTypeList = salesOrderViewModel.OrderWiseTaxViewList
            //    .Where(e => e.ApplicableAt == Shared.sFAModel.TaxApplicableType.TAXONTAX)
            //    .Select(e => e.TaxCalculationType).Distinct().ToList<string>();
            //foreach (string calculationType in calculationTypeList)
            //{
            //    if (calculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE)
            //    {
            //        List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> percentageOrderWiseTaxViewList = salesOrderViewModel.OrderWiseTaxViewList
            //                .Where(e => e.ApplicableAt == Shared.sFAModel.TaxApplicableType.TAXONTAX
            //                && e.TaxCalculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE)
            //                .ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();
            //        if (percentageOrderWiseTaxViewList != null && percentageOrderWiseTaxViewList.Count() > 0)
            //        {
            //            foreach (Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxView in percentageOrderWiseTaxViewList)
            //            {
            //                /*Get Total Taxable Amount For Dependent TaxUID & Calculate Tax on that*/
            //                List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> dependentOrderWiseTaxView = salesOrderViewModel.AppliedOrderWiseTaxViewList
            //                    .Where(e => e.TaxUID == orderWiseTaxView.DependentTaxUID)
            //                    .ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();
            //                if (dependentOrderWiseTaxView != null && dependentOrderWiseTaxView.Count() > 0)
            //                {
            //                    decimal dependentTotalValue = dependentOrderWiseTaxView.Sum(e => e.TaxAmount);
            //                    Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxViewClone = new Shared.sFAModel.Domain.Tax.OrderWiseTaxView(orderWiseTaxView);
            //                    if (salesOrderViewModel.IsPriceInclusiveVat)
            //                    {
            //                        orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem((dependentTotalValue
            //                            * orderWiseTaxViewClone.BaseTaxRate) / (100 + orderWiseTaxViewClone.BaseTaxRate));
            //                    }
            //                    else
            //                    {
            //                        orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem(dependentTotalValue
            //                            * orderWiseTaxViewClone.BaseTaxRate * Shared.Util.CommonFunction.GetDecimalValue(.01));
            //                    }

            //                    salesOrderViewModel.ProrataTaxAmount += orderWiseTaxViewClone.TaxAmount;
            //                    salesOrderViewModel.AppliedOrderWiseTaxViewList.Add(orderWiseTaxViewClone);
            //                }
            //            }
            //        }

            //    }
            //    else if (calculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE_SLAB)
            //    {
            //        List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> percentageOrderWiseTaxViewList = salesOrderViewModel.OrderWiseTaxViewList
            //                .Where(e => e.ApplicableAt == Shared.sFAModel.TaxApplicableType.TAXONTAX
            //                && e.TaxCalculationType == Shared.sFAModel.TaxCalculationType.PERCENTAGE_SLAB
            //                && salesOrderViewModel.TotalAmount >= e.RangeStart && salesOrderViewModel.TotalAmount < e.RangeEnd)
            //                .ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();
            //        if (percentageOrderWiseTaxViewList != null && percentageOrderWiseTaxViewList.Count() > 0)
            //        {
            //            foreach (Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxView in percentageOrderWiseTaxViewList)
            //            {
            //                /*Get Total Taxable Amount For Dependent TaxUID & Calculate Tax on that*/
            //                List<Shared.sFAModel.Domain.Tax.OrderWiseTaxView> dependentOrderWiseTaxView = salesOrderViewModel.AppliedOrderWiseTaxViewList
            //                    .Where(e => e.TaxUID == orderWiseTaxView.DependentTaxUID)
            //                    .ToList<Shared.sFAModel.Domain.Tax.OrderWiseTaxView>();

            //                if (dependentOrderWiseTaxView != null && dependentOrderWiseTaxView.Count() > 0)
            //                {
            //                    decimal dependentTotalValue = dependentOrderWiseTaxView.Sum(e => e.TaxAmount);
            //                    Shared.sFAModel.Domain.Tax.OrderWiseTaxView orderWiseTaxViewClone = new Shared.sFAModel.Domain.Tax.OrderWiseTaxView(orderWiseTaxView);
            //                    if (salesOrderViewModel.IsPriceInclusiveVat)
            //                    {
            //                        orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem((dependentTotalValue
            //                        * orderWiseTaxViewClone.TaxRate) / (100 + orderWiseTaxViewClone.TaxRate));
            //                    }
            //                    else
            //                    {
            //                        orderWiseTaxViewClone.TaxAmount = Shared.Util.CommonFunction.RoundForSystem(dependentTotalValue
            //                        * orderWiseTaxViewClone.TaxRate * Shared.Util.CommonFunction.GetDecimalValue(.01));
            //                    }

            //                    salesOrderViewModel.ProrataTaxAmount += orderWiseTaxViewClone.TaxAmount;
            //                    salesOrderViewModel.AppliedOrderWiseTaxViewList.Add(orderWiseTaxViewClone);
            //                }
            //            }
            //        }
            //    }
            //}
        }
        private void UpdateLineWiseProrataTax()
        {
            //if (salesOrderViewModel.AppliedOrderWiseTaxViewList != null && salesOrderViewModel.AppliedOrderWiseTaxViewList.Count() > 0)
            //{
            //    //if (AddProrataTaxesToDb() >= 0)
            //    //{
            //    decimal TotalAmountWithoutTax = salesOrderViewModel.SalesOrderItemViewListSelected.Sum(e => (e.TotalAmount - e.TotalDiscount));

            //    foreach (Shared.sFAModel.Domain.SOrder.SalesOrderItem salesOrderItem in salesOrderViewModel.SalesOrderItemViewListSelected)
            //    {
            //        // Distribute Prorata Tax Amount in ratio of total amount
            //        if (TotalAmountWithoutTax == 0)
            //        {
            //            salesOrderItem.ProrataTaxAmount = 0;
            //        }
            //        else
            //        {
            //            salesOrderItem.ProrataTaxAmount = Shared.Util.CommonFunction.RoundForSystem(((salesOrderItem.TotalAmount - salesOrderItem.TotalDiscount) / TotalAmountWithoutTax) * salesOrderViewModel.ProrataTaxAmount);
            //        }
            //        salesOrderItem.TotalTax = salesOrderItem.LineTaxAmount + salesOrderItem.ProrataTaxAmount;
            //        if (salesOrderViewModel.IsPriceInclusiveVat)
            //        {
            //            salesOrderItem.NetAmount = salesOrderItem.TotalAmount;
            //        }
            //        else
            //        {
            //            salesOrderItem.NetAmount = salesOrderItem.TotalAmount + salesOrderItem.TotalTax;
            //        }
            //        // Need to Handle Failed Case
            //    }
            //    //}
            //}
        }
    }
}
