using Hangfire.Console;
using Hangfire.Server;
using Serilog;
using SyncManagerBL.Interfaces;

namespace WINITSyncManager
{
    public class App
    {
        private readonly PullIntegration _pullIntegration;
        private readonly PushIntegration _pushIntegration;
        private readonly StagingInsertion _stagingInsertion;
        private readonly DataProcessing _dataProcessing;
        private readonly OracleStatusUpdateService _oracleStatusUpdate;
        private readonly Iint_CommonMethodsBL _intCommonMethodsBL;
        public App(PullIntegration pullIntegration, PushIntegration pushIntegration, StagingInsertion stagingInsertion,
            DataProcessing dataProcessing, OracleStatusUpdateService oracleStatusUpdate, Iint_CommonMethodsBL intCommonMethodsBL)
        {
            _pullIntegration = pullIntegration;
            _pushIntegration = pushIntegration;
            _stagingInsertion = stagingInsertion;
            _dataProcessing = dataProcessing;
            _oracleStatusUpdate = oracleStatusUpdate;
            _intCommonMethodsBL = intCommonMethodsBL;
        }

        public async Task RunPullProcess(string Entity, PerformContext context)
        {
            int isERPposting = await _intCommonMethodsBL.CheckisJobEnable(Entity);
            if (isERPposting != 1)
            {
                context?.WriteLine($"{Entity} Pull process stopped as Is Job Enable was disabled.");
                return;
            }
            var steps = new Dictionary<Func<Task>, string>
             {
                 { () => _pullIntegration.RunPullProcessByEntity(Entity,context), "Step 1: Fail to Pull Data From Oracle." },
                 { () => _stagingInsertion.RunStagingInsertionProcessByEntity(Entity, context), "Step 2: Fail to Insert Data Into Month Tables." },
                 { () => _dataProcessing.RunMainInsertionProcess(Entity, context), "Step 3: Fail to Insert Data Into Actual Tables." },
                 { () => _oracleStatusUpdate.RunOracleStatusUpdationProcess(Entity, context), "Step 4: Fail to Update Process Status In Oracle." }
             };
            var exceptions = new List<Exception>();
            foreach (var step in steps)
            {
                try
                {
                    await step.Key();
                }
                catch (Exception ex)
                {
                    context.WriteLine(step.Value);
                    context.WriteLine(ex.Message);
                    Log.Error(ex, step.Value);
                    exceptions.Add(new Exception($"{step.Value}: {ex.Message}", ex));
                }
            }
            // Throw aggregated exception if there are any failures
            if (exceptions.Count > 0)
            {
                throw new AggregateException("One or more steps failed during the process.", exceptions);
            }
        }
        public async Task RunPushProcess(string entity, PerformContext context)
        {
            context.WriteLine($"Step 1: Running {entity} Push Process.");
            try
            {
                int isERPposting = await _intCommonMethodsBL.CheckisJobEnable(entity);
                if (isERPposting != 1)
                {
                    context?.WriteLine($"{entity} Pull process stopped as Is Job Enable was disabled.");
                    return;
                }
                await _pushIntegration.RunPushProcess(entity, context);
            }
            catch (Exception ex)
            {
                context.WriteLine("Fail to Run Push Process");
                context.WriteLine(ex.Message);
                Log.Error(ex, "Fail to Run Push Process");
                throw;
            }
        }
    }
}
