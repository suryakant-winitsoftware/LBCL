using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Constants
{
    public class OrderType
    {
        public const string Presales = "Presales";
        public const string Vansales = "Vansales";
        public const string Cashsales = "Cashsales";
        public const string Forward = "Forward";
        public static readonly string[] CashVansales = { "Cashsales", "Vansales" };
        public const string FOC = "FOC";

    }
}
