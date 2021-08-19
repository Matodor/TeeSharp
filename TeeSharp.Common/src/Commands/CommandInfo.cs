namespace TeeSharp.Common.Commands
{
    public delegate void CommandHandler(CommandInfo commandInfo, CommandArgs args);
    
    public class CommandInfo
    {
        public const int MinCommandLength = 1;
        public const int MaxCommandLength = 32;
        public const int MaxDescriptionLength = 96;
        public const int MaxParamsLength = 16;
        
        public event CommandHandler Executed;
        
        public string ParametersPattern { get; }
        public string Description { get; set; }
        
        public CommandInfo(
            string parametersPattern, 
            string description)
        {
            ParametersPattern = parametersPattern;
            Description = description;
        }

        public void OnCommandExecuted(CommandArgs args)
        {
            Executed?.Invoke(this, args);
        }
    }
}