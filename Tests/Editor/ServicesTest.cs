
namespace TeaSpoons.ServiceLocator.Editor.Tests
{
    using NUnit.Framework;
    using System;

    public class ServicesTest
    {
        private interface ITestService : IService
        {
        }

        private class TestService : ITestService
        {
        }

        private class AlternativeTestService : ITestService
        {
        }

        [SetUp]
        public void SetUp()
        {
            ServicesTestMode.Start();
        }

        [Test]
        public void BindingAndGetting()
        {
            var instance = new TestService();
            ServiceBinder.For<ITestService>().Bind(() => instance);

            Assert.AreSame(instance, Services.Get<ITestService>());
        }

        [Test]
        public void DuplicateBindingTriggersException()
        {
            Action bind = () => ServiceBinder.For<ITestService>().Bind(() => new TestService());

            bind();

            Assert.Throws<DuplicateBindingException>(() => bind());

            bind = () => ServiceBinder.For<ITestService>().Bind(() => new AlternativeTestService());
            Assert.Throws<DuplicateBindingException>(() => bind());
        }

        [Test]
        public void MissingBindingTriggersException()
        {
            Assert.Throws<MissingServiceException>(() => Services.Get<ITestService>());
        }

        [Test]
        public void AddExpectation()
        {
            ServiceExpectations.Add<ITestService, TestService>();

            // Should fail silently
            ServiceBinder.For<ITestService>().Bind(() => new AlternativeTestService());
            Assert.Throws<MissingServiceException>(() => Services.Get<ITestService>());

            var intendedService = new TestService();
            ServiceBinder.For<ITestService>().Bind(() => intendedService);
            Assert.IsNotNull(Services.Get<ITestService>());

            // Should fail silently
            ServiceBinder.For<ITestService>().Bind(() => new AlternativeTestService());
            Assert.AreSame(intendedService, Services.Get<ITestService>());
        }
    }
}
