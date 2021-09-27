using System;

namespace TeeSharp.Common.Commands
{
    public class CommandArgs : EventArgs
    {
        public new static readonly CommandArgs Empty = new CommandArgs();
    }
}