using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants
{
    public class SalesOrderStatus
    {
        public const string DRAFT = "Draft";
        public const string APPROVAL_PENDING = "Approval Pending";
        public const string FINALIZED = "Finalized";
        public const string APPROVED = "Approved";
        public const string WAREHOUSE = "Warehouse";
        public const string DELIVERED = "Delivered";
        public const string REJECTED = "Rejected";
        public const string TOPUP = "Top up";
        public const string STANDING = "Standing";
        public const string TEMPORARY = "Temporary";
        public const string ASSIGNED = "Assigned";
        public const string ALLOCATED = "Allocated";
        public const string PENDING = "Pending"; // Added for showing label
        public const string SENT = "Sent"; // Added for showing label
        public const string ERROR = "Error"; // Added for showing label
        public const string RECEIVED = "Received";
        public const string DELETED = "Deleted";// Added for showing label
        public const string OPEN = "Open";// Added for showing label
        public const string PENDING_FROM_BM = "Pending from BM";// Added for showing label
        public const string PENDING_FROM_ASM = "Pending from ASM";// Added for showing label
    }
}
