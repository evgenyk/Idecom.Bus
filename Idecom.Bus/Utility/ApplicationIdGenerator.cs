using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Idecom.Bus.Utility
{
    public static class ApplicationIdGenerator
    {
        private static string _machineIdCache;

        /// <summary>
        ///     Generated a persistent ID for the running application bu using FQDN and current assembly entry point file name
        ///     Should ideally be persistent between restarts
        /// </summary>
        /// <returns></returns>
        [DebuggerNonUserCode] //so that exceptions don't jump at the developer debugging their app
        public static string GenerateIdId()
        {
            if (_machineIdCache != null)
                return _machineIdCache;

            Assembly entryAssembly = Assembly.GetEntryAssembly();

            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            string fqdn = "";
            if (!hostName.Contains(domainName))
                fqdn = hostName + "." + domainName;
            else
                fqdn = hostName;

            fqdn = fqdn.ToLowerInvariant();
            string directory = new Uri(entryAssembly.CodeBase).LocalPath.ToLowerInvariant();

            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(directory));
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(i.ToString("D"));
            string serviceId = sb.ToString();
            _machineIdCache = serviceId + "@" + fqdn;
            return serviceId;
        }
    }
}