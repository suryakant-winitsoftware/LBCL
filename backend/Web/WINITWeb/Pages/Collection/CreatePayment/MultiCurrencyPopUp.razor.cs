using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class MultiCurrencyPopUp
    {
        [Parameter] public bool IsShow { get; set; } = false;
        [Parameter] public EventCallback Closebtn { get; set; }
        [Parameter] public List<IExchangeRate> _currencyCls { get; set; } = new List<IExchangeRate>();
        public static List<IExchangeRate> _currencyClsDisplay { get; set; } = new List<IExchangeRate>();
        [Parameter] public decimal Invoice { get; set; } = 0;
        public decimal InvoiceCopy { get; set; } = 0;
        public decimal Invoice_Copy { get; set; } = 0;
        [Parameter] public decimal Collected { get; set; } = 0;
        [Parameter] public decimal Remaining { get; set; } = 0;
        [Parameter] public EventCallback<decimal> OnSubmitAmount { get; set; }
        [Parameter] public EventCallback<Dictionary<string, decimal>> OnSubmitPaymentAmount { get; set; }
        [Parameter] public EventCallback<Dictionary<string, List<IExchangeRate>>> OnSubmitList { get; set; }
        [Parameter] public static string PaymentType { get; set; } = "";
        public decimal CollectedAmount { get; set; } = 0;
        public static Dictionary<string, List<IExchangeRate>> StateManagement = new Dictionary<string, List<IExchangeRate>>();
        public List<IExchangeRate> CurrencyRateRecords { get; set; } = new List<IExchangeRate>();
        [Parameter] public bool IsReadOnly { get; set; } = false;
        public decimal maxLimit = 0;
        public decimal minLimit = 0;
        public string DefaultCurrency { get; set; } = "";
        public decimal MinMaxDiff = 0;

        public async Task OnInit(string PaymentMode)
        {
            PaymentType = PaymentMode;
            if (!IsReadOnly)
            {
                if (StateManagement.ContainsKey(PaymentMode))
                {
                    _currencyCls = StateManagement[PaymentMode];
                }
                else
                {
                    _currencyCls = await _createPaymentViewModel.GetCurrencyRateRecords("");
                }
            }
            foreach (var data in _currencyCls)
            {
                data.CurrencyAmount_Temp = data.CurrencyAmount;
                data.ConvertedAmount_Temp = data.ConvertedAmount;
            }
            //IsShow = true;
            await Task.CompletedTask;
            StateHasChanged();
        }
        public async Task Close()
        {
            try
            {
                await Closebtn.InvokeAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task CancelBtn()
        {
            try
            {
                await Closebtn.InvokeAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task OkCloseBtn()
        {
            try
            {
                if (!IsReadOnly)
                {
                    if (!await RoundOffCalculation(_currencyCls))
                    {
                        return;
                    }
                    foreach (var data in _currencyCls)
                    {
                        data.CurrencyAmount = data.CurrencyAmount_Temp;
                        data.ConvertedAmount = data.ConvertedAmount_Temp;
                        data.OriginalAmount = data.Rate * data.CurrencyAmount_Temp;
                    }
                    if (StateManagement.ContainsKey(PaymentType))
                    {
                        // If the key exists, update the list with the new value
                        StateManagement[PaymentType] = _currencyCls;
                    }
                    else
                    {
                        // If the key doesn't exist, insert the key-value pair into the dictionary
                        StateManagement[PaymentType] = _currencyCls;
                    }
                    _createPaymentViewModel.MultiCurrencyDetailsData = StateManagement;
                    decimal Amount = 0;
                    foreach (var data in StateManagement[PaymentType])
                    {
                        Amount += data.ConvertedAmount_Temp;
                    }
                    var paymentAmounts = new Dictionary<string, decimal>
                    {
                        { PaymentType,  Amount}
                    };
                    await OnSubmitPaymentAmount.InvokeAsync(paymentAmounts);
                    await OnSubmitAmount.InvokeAsync(Amount);
                }

                await Closebtn.InvokeAsync();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task AmountCalculation(ChangeEventArgs e, string Propertyname, IExchangeRate exchangeRate)
        {
            try
            {
                switch (Propertyname)
                {
                    case "CurrencyAmount":
                        exchangeRate.ConvertedAmount_Temp = Convert.ToDecimal(string.IsNullOrEmpty(e.Value?.ToString()) ? 0 : e.Value) * exchangeRate.Rate;
                        exchangeRate.CurrencyAmount_Temp = Convert.ToDecimal(string.IsNullOrEmpty(e.Value?.ToString()) ? 0 : e.Value);
                        exchangeRate.OriginalAmount = exchangeRate.Rate * exchangeRate.CurrencyAmount_Temp;
                        break;
                    default:
                        break;
                }
                //await UpdateCollectedAmount(exchangeRate);
                _currencyCls.FirstOrDefault().TotalAmount_Temp = CollectedAmount;
                StateHasChanged();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task AdjustMultiCurrencyAmount(string Mode)
        {
            try
            {
                if (StateManagement.ContainsKey(Mode))
                {
                    foreach (var record in StateManagement[Mode])
                    {
                        record.ConvertedAmount = 0;
                        record.ConvertedAmount_Temp = 0;
                        record.CurrencyAmount = 0;
                        record.CurrencyAmount_Temp = 0;
                        record.OriginalAmount = 0;
                        record.OriginalAmount_Temp = 0;
                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task ClearDictionary()
        {
            try
            {
                StateManagement.Clear();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<bool> RoundOffCalculation(List<IExchangeRate> exchangeRate)
        {
            try
            {
                if (exchangeRate.Count != 0)
                {
                    foreach (var data in exchangeRate)
                    {
                        maxLimit = _iAppUser.OrgCurrencyList.FirstOrDefault(p => p.UID == data.FromCurrencyUID).RoundOffMaxLimit;
                        minLimit = _iAppUser.OrgCurrencyList.FirstOrDefault(p => p.UID == data.FromCurrencyUID).RoundOffMinLimit;
                        DefaultCurrency = "INR";
                        await CalculateAmount(data);
                        if (DefaultCurrency == data.FromCurrencyUID && data.CurrencyAmount_Temp != data.ConvertedAmount_Temp)
                        {
                            await _alertService.ShowErrorAlert("Alert", "Can't round off Local Currency");
                            return false;
                        }
                        if ((MinMaxDiff) >= minLimit && (MinMaxDiff) <= maxLimit)
                        {

                        }
                        else
                        {
                            await _alertService.ShowErrorAlert("Alert", "Round off range should be " + minLimit + " to " + maxLimit + ".");
                            return false;
                        }
                    }
                }
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                await _alertService.ShowErrorAlert("Alert", "RoundOffCalculation Error");
                return false;
            }
        }
        public async Task<decimal> CalculateAmount(IExchangeRate exchangeRate)
        {
            try
            {
                MinMaxDiff = exchangeRate.OriginalAmount > exchangeRate.ConvertedAmount_Temp ?
                exchangeRate.OriginalAmount - exchangeRate.ConvertedAmount_Temp : exchangeRate.ConvertedAmount_Temp - exchangeRate.OriginalAmount;
                await Task.CompletedTask;
                return MinMaxDiff;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
