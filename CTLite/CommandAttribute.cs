using System;

namespace CTLite
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public CommandAttribute() { }
    }
}