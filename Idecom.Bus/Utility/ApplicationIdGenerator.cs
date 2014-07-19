namespace Idecom.Bus.Utility
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    public static class ApplicationIdGenerator
    {
        static string _machineIdCache;

        /// <summary>
        ///     Generated a persistent ID for the running application bu using FQDN and current assembly entry point file name
        ///     Should ideally be persistent between restarts
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode] //so that exceptions don't jump at the developer debugging their app
        public static string GenerateId()
        {
            if (_machineIdCache != null)
                return _machineIdCache;

            var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();
            var fqdn = "";
            if (!hostName.Contains(domainName))
                fqdn = hostName + "." + domainName;
            else
                fqdn = hostName;

            fqdn = fqdn.ToLowerInvariant();
            var directory = new Uri(entryAssembly.CodeBase).LocalPath.ToLowerInvariant();

            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(directory));
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                sb.Append(i.ToString("D"));
            var serviceId = sb.ToString();
            _machineIdCache = serviceId + "@" + fqdn;
            return _machineIdCache;
        }
    }
}