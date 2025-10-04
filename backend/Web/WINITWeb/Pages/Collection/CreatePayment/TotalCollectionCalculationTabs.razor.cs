using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace WinIt.Pages.Collection.CreatePayment
{
    public partial class TotalCollectionCalculationTabs : ComponentBase
    {
        [Parameter] public List<IAccCollection> DataSource { get; set; } = new List<IAccCollection>();
        public List<string> Status = new List<string> { "Submitted", "Settled", "Approved", "Collected" };
        public decimal CalculateTotalAmount()
        {
            try
            {
                return
                 DataSource.Where(p => Status.Any(status => status.Contains(p.Status))).Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
