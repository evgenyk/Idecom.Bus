namespace Idecom.Bus.Tests
{
    using Implementations;
    using Xunit;
    using Xunit.Should;

    public class InterfaceProxyGeneratorTests
    {
        [Fact]
        public void CanGenerateAndSerializeInterface()
        {
            var generator = new InstanceCreator();
            var instance = generator.CreateInstanceOf<IAmAnInterface>();
            instance.StringProperty = "data";
            instance.ShouldNotBe(null);
        }

        [Fact]
        public void CanCreateAnInstanceOfAClass()
        {
            var generator = new InstanceCreator();
            var instance = generator.CreateInstanceOf<SomeClass>();
            instance.StringProperty = "data";
            instance.ShouldNotBe(null);
        }


        public interface IAmAnInterface : IAnotherInterface
        {
            // ReSharper disable once CSharpWarnings::CS0108
            string StringProperty { get; set; } //hiding is intended here
        }

        public interface IAnotherInterface
        {
            string StringProperty { get; set; }
        }

        public class SomeClass
        {
            public string StringProperty { get; set; }
        }
    }
}