﻿using SimpleContainer.Tests.DummyTypes;

using NUnit.Framework;

namespace SimpleContainer.Tests
{
    [TestFixture]
    public class DispatcherTests
    {
        [Test]
        public void Dispatcher_Send_Singleton_Exact()
        {
            var container = Container.Create();

            container.Register<ICustomHandler, CustomHandler>(Scope.Singleton);
            container.Register<DummyInvoker>(Scope.Singleton);
            container.RegisterEvent<ICustomHandler, CustomArgs>((handler, args) => handler.OnCustomEvent(args));

            var invoker = container.Resolve<DummyInvoker>();
            var eventHandler = container.Resolve<ICustomHandler>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Send_Singleton_Shortcut()
        {
            var container = Container.Create();

            container.Register<ICustomHandler, CustomHandler>(Scope.Singleton);
            container.Register<DummyInvoker>(Scope.Singleton);
            container.RegisterEvent<ICustomHandler, CustomArgs>();

            var invoker = container.Resolve<DummyInvoker>();
            var eventHandler = container.Resolve<ICustomHandler>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Send_Singleton_Contract()
        {
            var container = Container.Create();

            container.Register<CustomContractHandler>(Scope.Singleton);
            container.Register<DummyContractInvoker>(Scope.Singleton);
            container.RegisterEvent<CustomContractArgs>();

            var invoker = container.Resolve<DummyContractInvoker>();
            var eventHandler = container.Resolve<CustomContractHandler>();

            var expectedValue = new CustomContractArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Send_Factory()
        {
            var container = Container.Create();

            container.Register<DummyInvoker>(Scope.Singleton);
            container.RegisterFactory<CustomHandlerFactory>();
            container.RegisterEvent<ICustomHandler, CustomArgs>((handler, args) => handler.OnCustomEvent(args));

            var invoker = container.Resolve<DummyInvoker>();
            var factory = container.Resolve<CustomHandlerFactory>();

            var eventHandler = factory.Create<CustomHandler>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Send_All()
        {
            var container = Container.Create();

            container.Register<DummyInvoker>(Scope.Singleton);
            container.Register<CustomHandler>(Scope.Singleton);
            container.RegisterEvent<ICustomHandler, CustomArgs>((handler, args) => handler.OnCustomEvent(args));

            var invoker = container.Resolve<DummyInvoker>();
            var eventHandler = container.Resolve<CustomHandler>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Handle_Any()
        {
            var container = Container.Create();

            container.Register<CustomAnyEventHandler>(Scope.Singleton);
            container.Register<DummyContractInvoker>(Scope.Singleton);
            container.RegisterEvent<CustomContractArgs>();

            var invoker = container.Resolve<DummyContractInvoker>();
            var eventHandler = container.Resolve<CustomAnyEventHandler>();

            var expectedValue = new CustomContractArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            var expectedEventType = typeof(CustomContractArgs);

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedArgs;
            var actualEventType = eventHandler.EventType;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedValue, actualValue);
                Assert.AreEqual(expectedEventType, actualEventType);
            });
        }

        [Test]
        public void Dispatcher_Handle_YieldInstruction()
        {
            var container = Container.Create();

            container.RegisterEvent<CustomArgs>();

            var dispatcher = container.Resolve<Dispatcher>();
            var instruction = dispatcher.CreateYieldInstruction<CustomArgs>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            Assert.IsNull(instruction.Current);

            instruction.MoveNext();

            Assert.IsNull(instruction.Current);

            dispatcher.Send(expectedValue);

            Assert.IsNull(instruction.Current);

            instruction.MoveNext();

            var actualValue = instruction.Current;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Handle_Callback()
        {
            var container = Container.Create();

            container.RegisterEvent<CustomArgs>();

            var dispatcher = container.Resolve<Dispatcher>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            CustomArgs actualValue = null;

            dispatcher.Subscribe<CustomArgs>(eventArgs =>
            {
                actualValue = eventArgs;
            });

            dispatcher.Send(expectedValue);

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Handle_Callback_Once()
        {
            var container = Container.Create();

            container.RegisterEvent<CustomArgs>();

            var dispatcher = container.Resolve<Dispatcher>();

            var expectedValue = new CustomArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            CustomArgs actualValue = null;
            int invokeCount = 0;

            dispatcher.SubscribeOnce<CustomArgs>(eventArgs =>
            {
                actualValue = eventArgs;
                invokeCount++;
            });

            dispatcher.Send(expectedValue);

            dispatcher.Send(new CustomArgs
            {
                flag = false,
                id = 10,
                name = "shutdown"
            });

            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(1, invokeCount);
        }
    }
}