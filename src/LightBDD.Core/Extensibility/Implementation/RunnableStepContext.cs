using System;
using System.Threading.Tasks;
using LightBDD.Core.Dependencies;
using LightBDD.Core.Execution.Implementation;
using LightBDD.Core.Extensibility.Results;
using LightBDD.Core.Metadata;
using LightBDD.Core.Notification;

namespace LightBDD.Core.Extensibility.Implementation
{
    internal class RunnableStepContext
    {
        public RunnableStepContext(ExceptionProcessor exceptionProcessor, IScenarioProgressNotifier progressNotifier,
            IDependencyContainer container, object context, ProvideStepsFunc provideSteps,
            Func<Exception, bool> shouldAbortSubStepExecution)
        {
            ExceptionProcessor = exceptionProcessor;
            ProgressNotifier = progressNotifier;
            Container = container;
            Context = context;
            ProvideSteps = provideSteps;
            ShouldAbortSubStepExecution = shouldAbortSubStepExecution;
        }

        public ExceptionProcessor ExceptionProcessor { get; }
        public IScenarioProgressNotifier ProgressNotifier { get; }
        public IDependencyContainer Container { get; }
        public object Context { get; }
        public ProvideStepsFunc ProvideSteps { get; }
        public Func<Exception, bool> ShouldAbortSubStepExecution { get; }
    }
}