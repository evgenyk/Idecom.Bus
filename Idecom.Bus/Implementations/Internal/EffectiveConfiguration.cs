namespace Idecom.Bus.Implementations.Internal
{
    using System;
    using System.Collections.Generic;

    internal interface IEffectiveConfiguration
    {
        Func<Type, bool> IsEvent { get; }
        Func<Type, bool> IsCommand { get; }
        Func<Type, bool> IsHandler { get; }
        List<NamespaceToEndpointMapping> NamespaceToEndpointMappings { get; }
    }

    internal class EffectiveConfiguration : IEffectiveConfiguration
    {
        public Func<Type, bool> IsEvent { get; set; }
        public Func<Type, bool> IsCommand { get; set; }
        public Func<Type, bool> IsHandler { get; set; }
        public List<NamespaceToEndpointMapping> NamespaceToEndpointMappings { get; set; }
    }
}