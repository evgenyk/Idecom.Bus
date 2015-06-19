namespace Idecom.Bus.Host
{
    using System.Collections.Generic;
    using Implementations;
    using Mono.Options;

    class Program
    {
        static void Main(string[] args)
        {
            string sourcePath = null;

            new OptionSet
            {
                {
                    "p|path=",
                    v=> sourcePath = v 
                }
            }.Parse(args);

            //Configure.With()
            
        }
        
    }
}