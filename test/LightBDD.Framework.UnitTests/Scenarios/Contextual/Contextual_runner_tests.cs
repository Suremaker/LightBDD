﻿using LightBDD.Core.Dependencies;
using LightBDD.Core.Extensibility;
using LightBDD.Framework.UnitTests.Scenarios.Helpers;
using Moq;
using NUnit.Framework;
using System;
using LightBDD.Framework.Scenarios;

namespace LightBDD.Framework.UnitTests.Scenarios.Contextual
{
    [TestFixture]
    public class Contextual_runner_tests
    {
        private class MyContext { }
        private Mock<ICoreScenarioBuilder> _builder;
        private IBddRunner _runner;

        [SetUp]
        public void SetUp()
        {
            _builder = ScenarioMocks.CreateScenarioBuilder();
            _runner = ScenarioMocks.CreateBddRunner(_builder);
        }

        [Test]
        public void It_should_allow_to_apply_context_instance()
        {
            var capture = _builder.ExpectContext();
            var context = new object();

            _runner.WithContext(context);

            _builder.Verify();
            Assert.That(capture.Value.Invoke(), Is.SameAs(context));
        }

        [Test]
        public void It_should_allow_to_apply_context_provider()
        {
            var capture = _builder.ExpectContext();

            _runner.WithContext(() => TimeSpan.FromSeconds(5));

            _builder.Verify();
            Assert.That(capture.Value.Invoke(), Is.EqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void It_should_allow_to_apply_context_resolved_with_DI_container()
        {
            var resolver = new Mock<IDependencyResolver>();
            var capture = _builder.ExpectResolvedContext();

            _runner.WithContext<MyContext>();

            _builder.Verify();
            capture.Value.Invoke(resolver.Object);
            resolver.Verify(x => x.Resolve(typeof(MyContext)));
        }

        [Test]
        public void It_should_allow_configuring_context_on_instantiation()
        {
            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.Resolve(typeof(ConfigurableContext)))
                .Returns(new ConfigurableContext { Dependency = "dep1" });

            var capture = _builder.ExpectResolvedContext();

            _runner.WithContext<ConfigurableContext>(ctx => ctx.Text = "foo");

            var instance = (ConfigurableContext)capture.Value.Invoke(resolver.Object);
            Assert.AreEqual("foo", instance.Text);
            Assert.AreEqual("dep1", instance.Dependency);
        }

        [Test]
        public void It_should_allow_configuring_context_on_instantiation_when_using_resolver()
        {
            var resolver = new Mock<IDependencyResolver>();
            resolver.Setup(r => r.Resolve(typeof(ConfigurableContext)))
                .Returns(new ConfigurableContext { Dependency = "dep1" });

            var capture = _builder.ExpectResolvedContext();

            _runner.WithContext(
                r => r.Resolve<ConfigurableContext>(),
                ctx => ctx.Text = "foo");

            var instance = (ConfigurableContext)capture.Value.Invoke(resolver.Object);
            Assert.AreEqual("foo", instance.Text);
            Assert.AreEqual("dep1", instance.Dependency);
        }

        class ConfigurableContext
        {
            public string Text { get; set; }
            public string Dependency { get; set; }
        }
    }
}
