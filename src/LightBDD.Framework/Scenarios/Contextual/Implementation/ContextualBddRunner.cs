using System;
using System.Diagnostics;
using LightBDD.Core.Extensibility;
using LightBDD.Framework.Extensibility;

namespace LightBDD.Framework.Scenarios.Contextual.Implementation
{
    [DebuggerStepThrough]
    internal class ContextualBddRunner<TContext> : IBddRunner<TContext>, IEnrichableFeatureFixtureRunner
    {
        private readonly IFeatureFixtureRunner _coreRunner;
        private readonly Func<object> _contextProvider;

        public ContextualBddRunner(IBddRunner inner, Func<object> contextProvider)
        {
            _contextProvider = contextProvider;
            _coreRunner = inner.Integrate();
        }

        public IScenarioRunner NewScenario() => _coreRunner.NewScenario().WithContext(_contextProvider);
        public TEnrichedRunner Enrich<TEnrichedRunner>(Func<IFeatureFixtureRunner, IIntegrationContext, TEnrichedRunner> runnerFactory)
        {
            return _coreRunner.AsEnrichable().Enrich(new ContextualRunnerEnricher<TEnrichedRunner>(this, runnerFactory).Enrich);
        }

        [DebuggerStepThrough]
        private class ContextualRunnerEnricher<TRunner>
        {
            private readonly IFeatureFixtureRunner _contextualRunner;
            private readonly Func<IFeatureFixtureRunner, IIntegrationContext, TRunner> _runnerFactory;

            public ContextualRunnerEnricher(IFeatureFixtureRunner contextualRunner, Func<IFeatureFixtureRunner, IIntegrationContext, TRunner> runnerFactory)
            {
                _contextualRunner = contextualRunner;
                _runnerFactory = runnerFactory;
            }

            public TRunner Enrich(IFeatureFixtureRunner _, IIntegrationContext ctx)
            {
                return _runnerFactory(_contextualRunner, ctx);
            }
        }
    }
}