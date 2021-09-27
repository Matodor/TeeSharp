using System.Collections.Generic;

namespace TeeSharp.Common.Commands
{
    public interface ICommandInfo
    {
        public IReadOnlyList<ParameterInfo> Parameters { get; }
        public CommandHandler Callback { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}