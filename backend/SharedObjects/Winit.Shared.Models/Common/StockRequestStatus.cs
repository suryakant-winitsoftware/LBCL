using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class StockRequestStatus
    {
        public const string Draft = "Draft";
        public const string Requested = "Requested";
        public const string Approved = "Approved";
        public const string Processed = "Processed";
        public const string Rejected = "Rejected";
        public const string Collected = "Collected";
    }
}
