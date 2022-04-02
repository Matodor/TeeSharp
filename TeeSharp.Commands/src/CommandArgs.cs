using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TeeSharp.Commands
{
    public class CommandArgs : EventArgs, IReadOnlyList<object>, IEquatable<CommandArgs>
    {
        public new static readonly CommandArgs Empty = new(Array.Empty<object>());

        public int Count => Arguments.Count;
        public object this[int index] => Arguments[index];

        protected readonly IReadOnlyList<object> Arguments;  
            
        public CommandArgs(IReadOnlyList<object> args)
        {
            Arguments = args;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public bool Equals(CommandArgs? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Arguments.SequenceEqual(other.Arguments);
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CommandArgs) obj);
        }

        public override int GetHashCode()
        {
            return Arguments.GetHashCode();
        }
    }
}
