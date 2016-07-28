using System;
using System.Linq;
using System.Threading.Tasks;
using LightBDD.Core.Execution.Results;
using LightBDD.Core.Execution.Results.Implementation;
using LightBDD.Core.Metadata.Implementation;
using LightBDD.Core.Notification;

namespace LightBDD.Core.Execution.Implementation
{
    internal class ScenarioExecutor
    {
        private readonly IProgressNotifier _progressNotifier;

        public ScenarioExecutor(IProgressNotifier progressNotifier)
        {
            _progressNotifier = progressNotifier;
        }

        public async Task Execute(ScenarioInfo scenario, Func<RunnableStep[]> stepsProvider, Func<object> contextProvider)
        {
            var scenarioContext = new ScenarioContext();
            try
            {
                ScenarioContext.Current = scenarioContext;
                await ExecuteWithinSynchronizationContext(scenario, stepsProvider, scenarioContext, contextProvider);
            }
            finally
            {
                ScenarioContext.Current = null;
            }
        }

        private async Task ExecuteWithinSynchronizationContext(ScenarioInfo scenario, Func<RunnableStep[]> stepsProvider, ScenarioContext scenarioContext, Func<object> contextProvider)
        {
            _progressNotifier.NotifyScenarioStart(scenario);

            var watch = ExecutionTimeWatch.StartNew();
            try
            {
                scenarioContext.InitializeScenario(stepsProvider, contextProvider);
                foreach (var step in scenarioContext.PreparedSteps)
                    await step.Invoke(scenarioContext, scenarioContext.ExecutionContext);
            }
            finally
            {
                watch.Stop();

                var result = new ScenarioResult(
                    scenario,
                    scenarioContext.PreparedSteps.Select(s => s.Result).ToArray(),
                    watch.GetTime(),
                    scenarioContext.ScenarioInitializationException);

                _progressNotifier.NotifyScenarioFinished(result);
                ScenarioExecuted?.Invoke(result);
            }
        }

        public event Action<IScenarioResult> ScenarioExecuted;
    }
}