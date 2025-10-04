using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.StoreCheck.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.StoreCheck.Model.Classes
{
    public class DamageReturnExpiry : IDamageReturnExpiry
    {
        public string DamageExpiryDate { get; set; }
        public string GoodReturnExpiryDate { get; set; }
        public string ExpiryDate { get; set; }
        public string DamageQty { get; set; }
        public string ExpireQty { get; set; }
        public string ReturnQty { get; set; }
        public string DamageNotes { get; set; }
        public string ExpireNotes { get; set; }
        public string ReturnNotes { get; set; }
        public List<ISelectionItem> ReturenReason { get; set; }

        public DamageReturnExpiry()
            {
                ReturenReason = new List<ISelectionItem>
                {
                    new SelectionItem { UID = "1", Code = "Reason1", Label = "Reason 1", IsSelected = false },
                    new SelectionItem { UID = "2", Code = "Reason2", Label = "Reason 2", IsSelected = false },
                    new SelectionItem { UID = "3", Code = "Reason3", Label = "Reason 3", IsSelected = false }
                };
            }
    }
    
}
