using Idecom.Bus.Implementations;
using Idecom.Bus.Utility;
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

        public class SomeClass
        {
            public string StringProperty { get; set; }
        }


        public interface IAmAnInterface : IAnotherInterface
        {
            string StringProperty { get; set; } //on purpose
        }

        public interface IAnotherInterface
        {
            string StringProperty { get; set; }
        }
    }
}