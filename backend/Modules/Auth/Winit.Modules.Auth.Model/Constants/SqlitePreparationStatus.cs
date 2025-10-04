using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Auth.Model.Constants
{
    public class SqlitePreparationStatus
    {
        public const string IN_PROCESS = "In Process";
        public const string READY = "Ready";
        public const string NOT_READY = "Not Ready";
        public const string FAILURE = "Failure";
    }
}
