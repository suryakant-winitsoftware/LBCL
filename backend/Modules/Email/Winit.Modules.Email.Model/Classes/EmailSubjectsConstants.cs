using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Email.Model.Classes
{
    public class EmailSubjectsConstants
    {
        public const string BMInfoSubject = "Purchase order {ORDER_NO} Created by {CHANNEL_PARTNER}";
        public const string CPInfoSubject = "Purchase order {ORDER_NO} Created by ASM {ASM_NAME}";
        public const string BMApprovalSubject = "Purchase order {ORDER_NO} Confirmed by {CHANNEL_PARTNER}";
        public const string ASMInfoSubject = "Purchase order {ORDER_NO} Approved by {APPROVED_BY}";
        public const string ASMConfirmationSubject = "Purchase order {ORDER_NO} Created by {CHANNEL_PARTNER}";
        public const string CPLastLevelApprovalSubject = "Purchase order {ORDER_NO} accepted";
        public const string InvoiceSubject = "Invoice {INVOICE_NO} generated for Purchase Order No {ORDER_NO}";
    }
}
