namespace Idecom.Host.Windows
{
    using System;
    using System.IO;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            var servicesBaseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            

        }
    }
}