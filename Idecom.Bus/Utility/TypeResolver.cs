namespace Idecom.Bus.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class TypeResolver
    {
        static readonly Dictionary<string, Type> ResolutionCache = new Dictionary<string, Type>(StringComparer.CurrentCultureIgnoreCase);

        public static Type ResolveType(string name)
        {
            var resolvedType = ResolveTypeInner(name);
            return resolvedType;
        }

        static Type ResolveTypeInner(string name)
        {
            var resolvedType = Type.GetType(name, false);
            if (resolvedType != null) return resolvedType;

            var typesCache = AssemblyScanner.GetTypes();

            Type value;
            if (ResolutionCache.TryGetValue(name, out value))
                return value;

            resolvedType = typesCache.FirstOrDefault(x => x.FullName.Equals(name));

            lock (ResolutionCache)
                if (!ResolutionCache.ContainsKey(name))
                    ResolutionCache[name] = resolvedType;

            if (resolvedType == null)
                throw new Exception($"Can not resolve type {name} for the purpose of deserialization.");

            return resolvedType;
        }
    }
}