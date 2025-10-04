using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Emp.Model.Interfaces;
using static Winit.Modules.CollectionModule.BL.Classes.CreatePayment.CreatePaymentAppViewModel;
using static WINITMobile.Pages.Collection.CollectPayment;

namespace WINITMobile.Pages.Collection
{
    public class Sample
    {
        private static Winit.Modules.CollectionModule.Model.Interfaces.ICollections collection = new Collections();
        private List<IAccPayable> SelectedItems { get; set; } = new List<IAccPayable>();
        public List<PaymentInfo> paymentInfos { get; set; } = new List<PaymentInfo>();
        public List<ICollections> CollectionListRecords { get; set; } = new List<ICollections>();
       
        public decimal TotalAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<decimal> Discounts { get; set; } = new List<decimal>();


        public async Task OnInitializedAsync()
        {
            var payablec = new AccPayable
            {
                Amount = 50,
                PayingAmount = 50,
                ReferenceNumber = "CREDITNOTE-1",
            };
            var payable = new AccPayable
            {
                Amount = 4000,
                PayingAmount = 4000,
                ReferenceNumber = "INVOICE-1",
            };
            var payable1 = new AccPayable
            {
                Amount = 2400,
                PayingAmount = 2400,
                ReferenceNumber = "INVOICE-2",
            };
            var info = new PaymentInfo
            {
                PaymentType = "Cash",
                IsChecked = true,
                Amount = 1400,
                //Type = "INVOICE-1",
            };
            var info1 = new PaymentInfo
            {
                PaymentType = "Cheque",
                IsChecked = true,
                Amount = 4950,
                //Type = "INVOICE-2",
            };
            SelectedItems.Add(payable);
            SelectedItems.Add(payable1);
            SelectedItems.Add(payablec);
            paymentInfos.Add(info);
            paymentInfos.Add(info1);
        }

        public async Task CreatePayment()
        {
            try
            {
                await OnInitializedAsync();

                //this.SelectedItems = SelectedItems;
                //this.paymentInfos = paymentInfos;
                //Amount = TotalAmount;

                foreach (var item in SelectedItems.Where(p => p.ReferenceNumber.Contains("CREDITNOTE", StringComparison.OrdinalIgnoreCase)))
                {
                    paymentInfos.Add(new PaymentInfo
                    {
                        PaymentType = item.ReferenceNumber,
                        IsChecked = true,
                        Amount = item.PayingAmount,
                        Type = item.ReferenceNumber,
                        // Set other properties as needed
                    });
                }
                foreach (var mode in paymentInfos.Where(p => p.IsChecked))
                {
                    collection.AccCollection = new AccCollection();
                    collection.AccPayable = new List<IAccPayable>();
                    collection.AccCollectionPaymentMode = new AccCollectionPaymentMode();
                    collection.AccStoreLedger = new AccStoreLedger();
                    collection.AccReceivable = new List<IAccReceivable>();
                    collection.AccCollectionAllotment = new List<IAccCollectionAllotment>();
                    await CreateAllotmentJSON(mode);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CreateAllotmentJSON(PaymentInfo Info)
        {
            try
            {
                collection.AccCollection = new AccCollection();
                AccReceivable recev = new AccReceivable();
                AccPayable pay = new AccPayable();
                 
                switch(Info.PaymentType)
                {
                    case "Cash":
                        TotalAmount = Info.Amount;
                        break;
                    case "Cheque":
                        TotalAmount = Info.Amount;
                        break;
                    case "POS":
                        TotalAmount = Info.Amount;
                        break;
                    case "Online":
                        TotalAmount = Info.Amount;
                        break;
                    default :
                        TotalAmount = 0;
                        break;
                }

                foreach (var item in SelectedItems.Where(p => p.edit))
                {
                    if (!Info.PaymentType.Contains("CREDITNOTE"))
                    {
                        await ApplyDiscount(item, Info.PaymentType);
                    }
                    if (Info.Amount > item.PayingAmount)
                    {
                        AccCollectionAllotment allotment = new AccCollectionAllotment();
                        allotment.PaidAmount = item.PayingAmount;
                        allotment.DiscountAmount = DiscountAmount;
                        allotment.ReferenceNumber = item.ReferenceNumber;
                        collection.AccCollectionAllotment.Add(allotment);
                        await CreateDiscountRecord(item, DiscountAmount);
                        await CreatePayableReceiveRecords(item, item.PayingAmount + DiscountAmount);
                        if (Info.Type.Contains("CREDITNOTE"))
                        {
                            await CreateCreditRecord(recev, Info);
                        }
                        Info.Amount -= item.PayingAmount;
                        item.PayingAmount = 0;
                        item.edit = false;
                    }
                    else
                    {
                        AccCollectionAllotment allotment = new AccCollectionAllotment();
                        allotment.PaidAmount = Info.Amount;
                        allotment.DiscountAmount = DiscountAmount;
                        allotment.ReferenceNumber = item.ReferenceNumber;
                        collection.AccCollectionAllotment.Add(allotment);
                        await CreateDiscountRecord(item, DiscountAmount);
                        await CreatePayableReceiveRecords(item, Info.Amount + DiscountAmount);
                        if (Info.Type.Contains("CREDITNOTE"))
                        {
                            await CreateCreditRecord(recev, Info);
                        }
                        item.PayingAmount -= Info.Amount;
                        Info.Amount = 0;
                        item.edit = item.PayingAmount == 0 ? false : true;
                        break;
                    }
                }
                DiscountAmount = 0;
                collection.AccCollection.Amount = TotalAmount - Discounts.Sum();
                CollectionListRecords.Add(collection);

                collection = new Collections();
                Discounts.Clear();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ApplyDiscount(IAccPayable item, string PaymentMode)
        {
            try
            {
                if (true)
                {
                    DiscountAmount = 0;
                    if (SelectedItems.Where(p => p.ReferenceNumber.Contains("INVOICE")).Last() == item)
                    {
                        CreditAmount += SelectedItems.Where(p => p.ReferenceNumber.Contains("CREDITNOTE")).Sum(p => p.PayingAmount);
                    }
                    if (item.Amount == item.PayingAmount && paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentMode).Amount >= item.PayingAmount - CreditAmount)
                    {
                        paymentInfos.FirstOrDefault(p => p.PaymentType == PaymentMode).Amount -= ((item.PayingAmount - CreditAmount) * 1.5m / 100);
                        DiscountAmount = (item.PayingAmount - CreditAmount) * 1.5m / 100;
                        Discounts.Add(DiscountAmount);
                        item.PayingAmount -= (item.PayingAmount - CreditAmount) * 1.5m / 100;
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CreatePayableReceiveRecords(IAccPayable accPayable, decimal PayingAmount)
        {
            try
            {
                if (accPayable.ReferenceNumber.Contains("INVOICE"))
                {
                    AccPayable pay = new AccPayable();
                    pay.ReferenceNumber = accPayable.ReferenceNumber;
                    pay.StoreUID = accPayable.StoreUID;
                    pay.SourceType = accPayable.SourceType;
                    pay.SourceUID = accPayable.SourceUID;
                    pay.PaidAmount = PayingAmount;
                    collection.AccPayable.Add(pay);
                }
                else
                {
                    AccReceivable receivable = new AccReceivable();
                    receivable.ReferenceNumber = accPayable.ReferenceNumber;
                    receivable.StoreUID = accPayable.StoreUID;
                    receivable.PaidAmount = PayingAmount;
                    receivable.SourceType = accPayable.SourceType;
                    receivable.SourceUID = accPayable.SourceUID;
                    collection.AccReceivable.Add(receivable);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CreateCreditRecord(IAccReceivable recev, PaymentInfo info)
        {
            try
            {
                AccCollectionAllotment allotment = new AccCollectionAllotment();
                IAccPayable ReceiveRecords = SelectedItems.FirstOrDefault(item => item.ReferenceNumber == info.PaymentType);
                recev.ReferenceNumber = ReceiveRecords.ReferenceNumber;
                recev.StoreUID = ReceiveRecords.StoreUID;
                recev.PaidAmount = ReceiveRecords.PayingAmount;
                recev.SourceType = ReceiveRecords.SourceType;
                recev.SourceUID = ReceiveRecords.SourceUID;
                allotment.UID = (Guid.NewGuid()).ToString();
                allotment.StoreUID = ReceiveRecords.StoreUID;
                allotment.TargetType = ReceiveRecords.SourceType;
                allotment.ReferenceNumber = ReceiveRecords.ReferenceNumber;
                allotment.PaidAmount = ReceiveRecords.PayingAmount;
                collection.AccReceivable.Add(recev);
                collection.AccCollectionAllotment.Add(allotment);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task CreateDiscountRecord(IAccPayable accPayable, decimal DiscountAmount)
        {
            try
            {
                if (DiscountAmount != 0)
                {
                    AccCollectionAllotment allotment = new AccCollectionAllotment();
                    allotment.UID = (Guid.NewGuid()).ToString();
                    allotment.PaidAmount = DiscountAmount;
                    allotment.DiscountAmount = 0;
                    allotment.TargetType = "CREDITNOTE";
                    allotment.StoreUID = accPayable.StoreUID;
                    allotment.ReferenceNumber = "";
                    collection.AccCollectionAllotment.Add(allotment);
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}

