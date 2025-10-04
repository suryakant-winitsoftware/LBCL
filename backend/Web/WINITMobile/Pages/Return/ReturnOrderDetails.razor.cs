using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WINITMobile.Pages.Return;

partial class ReturnOrderDetails
{
    [Parameter]
    public string ReturnOrderUID { get; set; }
    private Winit.Modules.ReturnOrder.Model.Interfaces.IReturnOrderInvoiceMaster? ReturnOrderInoiceMaster = null;

    protected async override Task OnInitializedAsync()
    {
		try
		{
            ReturnOrderInoiceMaster = await _returnOrderBL.GetReturnOrderInvoiceMasterByUID(ReturnOrderUID);
        }
		catch (Exception)
		{

			throw;
		}
    }
}
