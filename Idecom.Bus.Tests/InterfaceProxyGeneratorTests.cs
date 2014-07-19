using Idecom.Bus.Implementations;
using Xunit;
using Xunit.Should;

namespace Idecom.Bus.Tests
{
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
#pragma warning disable 108,114
            string StringProperty { get; set; }
#pragma warning restore 108,114
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