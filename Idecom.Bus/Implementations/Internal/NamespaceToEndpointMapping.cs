namespace Idecom.Bus.Implementations.Internal
{
    using Addressing;

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