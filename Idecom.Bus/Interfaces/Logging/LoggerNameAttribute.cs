namespace Idecom.Bus.Interfaces.Logging
{
    using System;

    public class LoggerNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public LoggerNameAttribute(string name)
        {
            Name = name;
        }
    }
}