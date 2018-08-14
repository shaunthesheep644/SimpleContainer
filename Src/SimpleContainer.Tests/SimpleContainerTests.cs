﻿using SimpleContainer.Tests.DummyTypes;

using NUnit.Framework;

namespace SimpleContainer.Tests
{
    [TestFixture]
    public class SimpleContainerTests
    {
        [Test]
        public void Resolve_Transient_SingleResult()
        {
            var container = Container.Create();

            container.Register<ICar, CarTruck>(Scope.Transient);

            var carFirst = container.Resolve<ICar>();
            var carSecond = container.Resolve<ICar>();

            Assert.AreNotSame(carFirst, carSecond);
        }

        [Test]
        public void Resolve_Transient_MultipleResult_Generic()
        {
            const int EXPECTED_COUNT = 2;

            var container = Container.Create();

            container.Register<IColor>(typeof(ColorRed), typeof(ColorBlue));

            var colors = container.ResolveMultiple<IColor>();
            var actualCount = colors.Length;

            Assert.AreEqual(EXPECTED_COUNT, actualCount);
        }

        [Test]
        public void Resolve_Transient_MultipleResult_Type_Explicit()
        {
            const int EXPECTED_COUNT = 2;

            var container = Container.Create();

            container.Register<IColor>(typeof(ColorRed), typeof(ColorBlue));

            var colors = container.ResolveMultiple(typeof(IColor));
            var actualCount = colors.Length;

            Assert.AreEqual(EXPECTED_COUNT, actualCount);
        }

        [Test]
        public void Resolve_Transient_MultipleResult_Type_Implicit()
        {
            const int EXPECTED_COUNT = 2;

            var container = Container.Create();

            container.Register<IColor>(typeof(ColorRed), typeof(ColorBlue));

            var colorsObject = container.Resolve(typeof(IColor[]));
            var colors = (object[])colorsObject;
            var actualCount = colors.Length;

            Assert.AreEqual(EXPECTED_COUNT, actualCount);
        }

        [Test]
        public void Resolve_Singleton_Contract()
        {
            var container = Container.Create();

            container.Register<ITimeMachine, TimeMachineDelorean>(Scope.Singleton);

            var machineFirst = container.Resolve<ITimeMachine>();
            var machineSecond = container.Resolve<ITimeMachine>();

            Assert.AreSame(machineFirst, machineSecond);
        }

        [Test]
        public void Resolve_Singleton_Result()
        {
            var container = Container.Create();

            container.Register<TimeMachineDelorean>(Scope.Singleton);

            var machineFirst = container.Resolve<TimeMachineDelorean>();
            var machineSecond = container.Resolve<TimeMachineDelorean>();

            Assert.AreSame(machineFirst, machineSecond);
        }

        [Test]
        public void Resolve_Singleton_Result_Throws_TypeNotRegisteredException()
        {
            var container = Container.Create();

            container.Register<ITimeMachine, TimeMachineDelorean>(Scope.Singleton);

            Assert.Throws(typeof(TypeNotRegisteredException), () =>
            {
                container.Resolve<TimeMachineDelorean>();
            });
        }

        [Test]
        public void Resolve_Factory()
        {
            var container = Container.Create();

            container.RegisterFactory<CarFactory>();

            var factory = container.Resolve<CarFactory>();
            var result = factory.Create();

            Assert.IsInstanceOf<ICar>(result);
        }

        [Test]
        public void Inject_Transient_SingleResult()
        {
            var container = Container.Create();

            container.Register<IEngine, EngineBig>(Scope.Transient);
            container.Register<ICar, CarFourWheelDrive>(Scope.Transient);

            var carFirst = container.Resolve<ICar>();
            var carSecond = container.Resolve<ICar>();

            Assert.AreNotSame(carFirst.Engine, carSecond.Engine);
        }

        [Test]
        public void Inject_Transient_MultipleResult()
        {
            const int EXPECTED_COUNT = 2;

            var container = Container.Create();

            container.Register<IColor>(typeof(ColorRed), typeof(ColorBlue));
            container.Register<IColorPalette, ColorPalette>(Scope.Singleton);

            var palette = container.Resolve<IColorPalette>();
            var actualCount = palette.Colors.Length;

            Assert.AreEqual(EXPECTED_COUNT, actualCount);
        }

        [Test]
        public void Inject_Singleton()
        {
            var container = Container.Create();

            container.Register<IPhysics, PhysicsPlanetEarth>(Scope.Singleton);
            container.Register<IEngine, EngineMedium>(Scope.Transient);

            var enigneFirst = container.Resolve<IEngine>();
            var enigneSecond = container.Resolve<IEngine>();

            Assert.AreSame(enigneFirst.Physics, enigneSecond.Physics);
        }

        [Test]
        public void Inject_Factory()
        {
            var container = Container.Create();

            container.Register<IEngine, EngineBig>(Scope.Transient);
            container.Register<ICar, CarFourWheelDrive>(Scope.Transient);

            container.RegisterFactory<CarFactory>();

            var factory = container.Resolve<CarFactory>();
            var car = factory.Create(typeof(CarFourWheelDrive));

            Assert.IsInstanceOf<EngineBig>(car.Engine);
        }

        [Test]
        public void Dispatcher_Send_Singleton()
        {
            var container = Container.Create();

            container.Register<ICustomEventHandler, CustomEventHandler>(Scope.Singleton);
            container.Register<DummyInvoker>(Scope.Singleton);

            container.RegisterEvent<ICustomEventHandler, CustomEventArgs>((handler, args) => handler.OnCustomEvent(args));

            var invoker = container.Resolve<DummyInvoker>();
            var eventHandler = container.Resolve<ICustomEventHandler>();

            var expectedValue = new CustomEventArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedEventArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Dispatcher_Send_Factory()
        {
            var container = Container.Create();

            container.Register<DummyInvoker>(Scope.Singleton);
            container.RegisterFactory<CustomEventHandlerFactory>();
            container.RegisterEvent<ICustomEventHandler, CustomEventArgs>((handler, args) => handler.OnCustomEvent(args));

            var invoker = container.Resolve<DummyInvoker>();
            var factory = container.Resolve<CustomEventHandlerFactory>();

            var eventHandler = factory.Create();

            var expectedValue = new CustomEventArgs
            {
                flag = true,
                id = 9,
                name = "shine"
            };

            invoker.RaiseEvent(expectedValue);

            var actualValue = eventHandler.ReceivedEventArgs;

            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}