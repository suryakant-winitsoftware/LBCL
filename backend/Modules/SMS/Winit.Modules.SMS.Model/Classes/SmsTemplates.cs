using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.SMS.Model.Classes
{
    public enum SmsTemplates
    {
        INVOICE_RECEIVED_FROM_ORACLE_SEND_FOR_INFO = 1,
        PO_APPROVED_BY_LAST_LEVEL_SEND_TO_CP_FOR_INFO,
        PO_CREATED_BY_CP_SEND_TO_BM_FOR_INFO,
        PO_CREATED_BY_CP_SEND_TO_ASM_FOR_APPROVAL,
        PO_APPROVED_BY_BM_SEND_TO_ASM_FOR_INFO,
        PO_APPROVED_BY_CP_SEND_TO_BM_FOR_APPROVAL,
        PO_CREATED_BY_ASM_SEND_TO_BM_FOR_INFO,
        PO_CREATED_BY_ASM_SEND_TO_CP_FOR_APPROVAL
    }
}
