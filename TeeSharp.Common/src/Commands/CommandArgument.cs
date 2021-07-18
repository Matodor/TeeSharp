using System;

namespace TeeSharp.Common.Commands
{
    public class CommandArgument
    {
        public CommandArgument(Type type, object value)
        {
            Type = type;
            Value = value;
        }

        public Type Type { get; set; }
        public object Value { get; set; }
    }
}