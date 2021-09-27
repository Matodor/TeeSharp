using System.Collections.Generic;
using System.Threading.Tasks;
using TeeSharp.Common.Commands.Builders;

namespace TeeSharp.Common.Commands
{
    public delegate Task CommandHandler(ICommandResult result);
    
    public class CommandInfo : ICommandInfo
    {
        public const int MinCommandLength = 1;
        public const int MaxCommandLength = 32;
        public const int MaxDescriptionLength = 96;
        public const int MaxParamsLength = 16;
        
        public IReadOnlyList<ParameterInfo> Parameters { get; }
        public CommandHandler Callback { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        internal CommandInfo(CommandBuilder builder)
        {
        }
    }
}