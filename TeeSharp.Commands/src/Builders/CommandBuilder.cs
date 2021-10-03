using System;
using System.Collections.Generic;

namespace TeeSharp.Commands.Builders
{
    public class CommandBuilder : ICommandInfo
    {
        public IReadOnlyList<ParameterInfo> Parameters => throw new NotImplementedException();

        public CommandHandler Callback { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ParameterBuilder> ParametersBuilders { get; set; }
        
        public CommandBuilder()
        {
            ParametersBuilders = new List<ParameterBuilder>();
        }

        public CommandBuilder WithParam(Action<ParameterBuilder> factory)
        {
            var builder = new ParameterBuilder();
            factory(builder);
            ParametersBuilders.Add(builder);
            return this;
        }
        
        public CommandBuilder WithName(string name)
        {
            Name = name;
            return this;
        }
        
        public CommandBuilder WithDescription(string desc)
        {
            Description = desc;
            return this;
        }
        
        public CommandBuilder WithCallback(CommandHandler callback)
        {
            Callback = callback;
            return this;
        }
        
        public CommandInfo Build()
        {
            return new CommandInfo(this);
        }
    }
}