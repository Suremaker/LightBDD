using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LightBDD.Core.Execution;
using LightBDD.Core.Execution.Dependencies;
using LightBDD.Core.Execution.Implementation;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Core.Extensibility.Execution.Implementation;
using LightBDD.Core.Extensibility.Results;
using LightBDD.Core.Internals;
using LightBDD.Core.Metadata;
using LightBDD.Core.Metadata.Implementation;
using LightBDD.Core.Notification;

namespace LightBDD.Core.Extensibility.Implementation
{
    [DebuggerStepThrough]
    internal class ScenarioRunner : IScenarioRunner
    {
        private readonly ScenarioExecutor _scenarioExecutor;
        private readonly IMetadataProvider _metadataProvider;
        private readonly IScenarioProgressNotifier _progressNotifier;
        private IEnumerable<StepDescriptor> _steps = Enumerable.Empty<StepDescriptor>();
        private INameInfo _name;
        private string[] _labels = Arrays<string>.Empty();
        private string[] _categories = Arrays<string>.Empty();
        private ExecutionContextDescriptor _contextDescriptor = ExecutionContextDescriptor.NoContext;
        private readonly ExceptionProcessor _exceptionProcessor;
        private readonly IDependencyContainer _container;
        private IEnumerable<IScenarioDecorator> _scenarioDecorators = Enumerable.Empty<IScenarioDecorator>();

        public ScenarioRunner(ScenarioExecutor scenarioExecutor, IMetadataProvider metadataProvider, IScenarioProgressNotifier progressNotifier, ExceptionProcessor exceptionProcessor, IDependencyContainer container)
        {
            _scenarioExecutor = scenarioExecutor;
            _metadataProvider = metadataProvider;
            _progressNotifier = progressNotifier;
            _exceptionProcessor = exceptionProcessor;
            _container = container;
        }

        public IScenarioRunner WithSteps(IEnumerable<StepDescriptor> steps)
        {
            if (_steps == null)
                throw new ArgumentNullException(nameof(steps));
            _steps = steps;
            return this;
        }

        public IScenarioRunner WithCapturedScenarioDetails()
        {
            var scenario = _metadataProvider.CaptureCurrentScenario();
            return WithName(_metadataProvider.GetScenarioName(scenario))
                .WithLabels(_metadataProvider.GetScenarioLabels(scenario.MethodInfo))
                .WithCategories(_metadataProvider.GetScenarioCategories(scenario.MethodInfo))
                .WithScenarioDecorators(_metadataProvider.GetScenarioDecorators(scenario));
        }

        public IScenarioRunner WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Unable to create scenario without name", nameof(name));
            _name = new NameInfo(name, Arrays<INameParameterInfo>.Empty());
            return this;
        }

        public IScenarioRunner WithContext(Func<object> contextProvider)
        {
            return WithContext(contextProvider, false);
        }

        public IScenarioRunner WithContext(Func<object> contextProvider, bool takeOwnership)
        {
            _contextDescriptor = new ExecutionContextDescriptor(contextProvider, takeOwnership);
            return this;
        }

        public IScenarioRunner WithContext(Func<IDependencyResolver, Task<object>> contextProvider)
        {
            _contextDescriptor = new ExecutionContextDescriptor(contextProvider);
            return this;
        }

        public IScenarioRunner WithScenarioDecorators(IEnumerable<IScenarioDecorator> scenarioDecorators)
        {
            _scenarioDecorators = scenarioDecorators ?? throw new ArgumentNullException(nameof(scenarioDecorators));
            return this;
        }

        private IScenarioRunner WithName(INameInfo name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        private void Validate()
        {
            if (_name == null)
                throw new InvalidOperationException("Scenario name is not provided.");
            if (!_steps.Any())
                throw new InvalidOperationException("At least one step has to be provided");
        }

        public IScenarioRunner WithLabels(string[] labels)
        {
            _labels = labels ?? throw new ArgumentNullException(nameof(labels));
            return this;
        }

        public IScenarioRunner WithCategories(string[] categories)
        {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            return this;
        }

        public async Task RunAsynchronously()
        {
            try
            {
                await RunScenarioAsync();
            }
            catch (ScenarioExecutionException e)
            {
                e.GetOriginal().Throw();
            }
        }

        public Task RunScenarioAsync()
        {
            Validate();
            return _scenarioExecutor.ExecuteAsync(new ScenarioInfo(_name, _labels, _categories), ProvideSteps, _contextDescriptor, _progressNotifier, _scenarioDecorators, _exceptionProcessor, _container);
        }

        public void RunSynchronously()
        {
            try
            {
                RunScenario();
            }
            catch (ScenarioExecutionException e)
            {
                e.GetOriginal().Throw();
            }
        }

        public void RunScenario()
        {
            var task = RunScenarioAsync();
            if (!task.IsCompleted)
                throw new InvalidOperationException("Only steps being completed upon return can be run synchronously (all steps have to return completed task). Consider using Async scenario methods for async Task or async void steps.");
            task.GetAwaiter().GetResult();
        }

        private RunnableStep[] ProvideSteps(DecoratingExecutor decoratingExecutor, object scenarioContext, IDependencyContainer container)
        {
            return ProvideSteps(decoratingExecutor, scenarioContext, container, _steps.ToArray(), string.Empty);
        }

        private RunnableStep[] ProvideSteps(DecoratingExecutor decoratingExecutor, object scenarioContext, IDependencyContainer container, StepDescriptor[] steps, string groupPrefix)
        {
            var totalStepsCount = steps.Length;
            string previousStepTypeName = null;
            var result = new RunnableStep[totalStepsCount];

            for (var i = 0; i < totalStepsCount; ++i)
            {
                var step = ToRunnableStep(steps[i], i, totalStepsCount, previousStepTypeName, decoratingExecutor, scenarioContext, container, groupPrefix);
                result[i] = step;
                previousStepTypeName = step.Result.Info.Name.StepTypeName?.OriginalName;
            }

            return result;
        }

        private RunnableStep ToRunnableStep(StepDescriptor descriptor, int stepIndex, int totalStepsCount,
            string previousStepTypeName, DecoratingExecutor decoratingExecutor, object scenarioContext,
            IDependencyContainer container, string groupPrefix)
        {
            var stepInfo = new StepInfo(_metadataProvider.GetStepName(descriptor, previousStepTypeName), stepIndex + 1, totalStepsCount, groupPrefix);
            var arguments = descriptor.Parameters.Select(p => new MethodArgument(p, _metadataProvider.GetValueFormattingServiceFor(p.ParameterInfo))).ToArray();
            var stepGroupPrefix = $"{stepInfo.GroupPrefix}{stepInfo.Number}.";
            return new RunnableStep(
                stepInfo,
                new InvocationResultTransformer(this, container, descriptor.StepInvocation, decoratingExecutor, stepGroupPrefix).InvokeAsync,
                arguments,
                _exceptionProcessor,
                _progressNotifier,
                decoratingExecutor,
                scenarioContext,
                _metadataProvider.GetStepDecorators(descriptor));
        }


        [DebuggerStepThrough]
        private struct InvocationResultTransformer
        {
            private readonly ScenarioRunner _runner;
            private readonly IDependencyContainer _container;
            private readonly Func<object, object[], Task<IStepResultDescriptor>> _invocation;
            private readonly DecoratingExecutor _decoratingExecutor;
            private readonly string _groupPrefix;

            public InvocationResultTransformer(ScenarioRunner runner, IDependencyContainer container, Func<object, object[], Task<IStepResultDescriptor>> invocation, DecoratingExecutor decoratingExecutor, string groupPrefix)
            {
                _runner = runner;
                _container = container;
                _invocation = invocation;
                _decoratingExecutor = decoratingExecutor;
                _groupPrefix = groupPrefix;
            }

            public async Task<CompositeStepContext> InvokeAsync(object context, object[] args)
            {
                var result = await _invocation.Invoke(context, args);

                if (!(result is CompositeStepResultDescriptor compositeDescriptor))
                    return CompositeStepContext.Empty;

                //TODO:handle instantiation failure
                var subStepScope = _container.BeginScope();
                var subStepsContext = await InstantiateSubStepsContextAsync(compositeDescriptor.SubStepsContext, subStepScope);
                try
                {
                    return new CompositeStepContext(subStepScope, _runner.ProvideSteps(_decoratingExecutor, subStepsContext, subStepScope, compositeDescriptor.SubSteps.ToArray(), _groupPrefix));
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Sub-steps initialization failed: {e.Message}", e);
                }
            }
        }

        private static async Task<object> InstantiateSubStepsContextAsync(ExecutionContextDescriptor contextDescriptor, IDependencyContainer container)
        {
            try
            {
                return await contextDescriptor.ContextResolver(container);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Sub-steps context initialization failed: {e.Message}", e);
            }
        }
    }
}