using Idecom.Bus.Addressing;

namespace Idecom.Bus.Implementations.Internal
{
    internal class NamespaceToEndpointMapping
    {
        public NamespaceToEndpointMapping(string ns, Address address)
        {
            Namespace = ns;
            Address = address;
        }

        public string Namespace { get; set; }
        public Address Address { get; set; }
    }
}