namespace Idecom.Bus.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class AssemblyScanner
    {
        private static List<Type> _typesCache = new List<Type>();

        /// <summary>
        ///     Gets a list with assemblies that can be scanned
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode] //so that exceptions don't jump at the developer debugging their app
        public static IEnumerable<Assembly> GetScannableAssemblies()
        {
            var directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var assemblyFiles = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories).Union(directoryInfo.GetFiles("*.exe", SearchOption.AllDirectories));


            foreach (FileInfo assemblyFile in assemblyFiles)
            {
                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    assembly.GetTypes();
                }

                catch (Exception) {
                    continue;
                }

                yield return assembly;
            }
        }

        [DebuggerNonUserCode]
        public static IEnumerable<Type> GetTypes()
        {
            if (_typesCache.Any()) return _typesCache;

            lock (_typesCache)
                _typesCache = _typesCache.Any() ? _typesCache : GetScannableAssemblies().SelectMany(x => x.GetTypes()).ToList();
            return _typesCache;
        }
    }
}