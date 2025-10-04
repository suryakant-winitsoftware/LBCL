using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
namespace Winit.Modules.StoreCheck.Model.Interfaces
{
    public interface IDamageReturnExpiry
    {
        string DamageExpiryDate { get; set; }
        string GoodReturnExpiryDate { get; set; }
        string ExpiryDate { get; set; }
        string DamageQty { get; set; }
        string ExpireQty { get; set; }
        string ReturnQty { get; set; }
        string DamageNotes { get; set; }
        string ExpireNotes { get; set; }
        string ReturnNotes { get; set; }
        List<ISelectionItem> ReturenReason { get; set; }
    }
}
