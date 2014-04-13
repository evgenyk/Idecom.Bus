using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Idecom.Bus.Utility
{
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
            FileInfo[] assemblyFiles = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles("*.*", SearchOption.AllDirectories);


            foreach (FileInfo assemblyFile in assemblyFiles)
            {
                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(assemblyFile.FullName);

                    //will throw if assembly can't be loaded
                    assembly.GetTypes();
                }

                catch (Exception)
                {
                    continue;
                }

                yield return assembly;
            }
        }

        [DebuggerNonUserCode]
        public static IEnumerable<Type> GetTypes()
        {
            if (!_typesCache.Any())
                lock (_typesCache)
                    _typesCache = _typesCache.Any() ? _typesCache : GetScannableAssemblies().SelectMany(x => x.GetTypes()).ToList();
            return _typesCache;
        }
    }
}