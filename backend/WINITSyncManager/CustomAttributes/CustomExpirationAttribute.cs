using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace WINITSyncManager.CustomAttributes
{
    public class CustomExpirationAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private readonly TimeSpan _expiration;

        public CustomExpirationAttribute(TimeSpan expiration)
        {
            _expiration = expiration;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = _expiration;
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // Not used in this case
        }
    }
}
