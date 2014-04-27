using System;
using System.Collections.Generic;

namespace Idecom.Bus.Implementations.Internal
{
    internal interface IEffectiveConfiguration
    {
        Func<Type, bool> IsEvent { get;  }
        Func<Type, bool> IsCommand { get;  }
        List<NamespaceToEndpointMapping> MessageMappings { get;  }
    }

    internal class EffectiveConfiguration : IEffectiveConfiguration
    {
        public Func<Type, bool> IsEvent { get; set; }
        public Func<Type, bool> IsCommand { get; set; }
        public List<NamespaceToEndpointMapping> MessageMappings { get; set; }
    }

}