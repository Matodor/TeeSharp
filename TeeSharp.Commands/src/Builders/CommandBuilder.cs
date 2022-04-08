using System;
using System.Collections.Generic;
using System.Linq;

namespace TeeSharp.Commands.Builders;

public class CommandBuilder
{
    internal List<ParameterBuilder> Parameters { get; set; }
    internal CommandHandler? Callback { get; set; }
    internal string? Name { get; set; }
    internal string? Description { get; set; }

    public CommandBuilder()
    {
        Parameters = new List<ParameterBuilder>();
    }

    public CommandBuilder WithParam(string pattern, Action<ParameterBuilder> factory)
    {
        var builder = ParameterBuilder.FromPattern(pattern);
        factory(builder);
        Parameters.Add(builder);
        return this;
    }

    public CommandBuilder WithParam(Action<ParameterBuilder> factory)
    {
        var builder = new ParameterBuilder();
        factory(builder);
        Parameters.Add(builder);
        return this;
    }

    public CommandBuilder WithName(string name)
    {
        Name = name.Trim();
        return this;
    }

    public CommandBuilder WithDescription(string desc)
    {
        Description = desc.Trim();
        return this;
    }

    public CommandBuilder WithCallback(CommandHandler callback)
    {
        Callback = callback;
        return this;
    }

    public CommandInfo Build()
    {
        if (string.IsNullOrEmpty(Name))
            throw new NullReferenceException(nameof(Name));

        switch (Name.Length)
        {
            case < CommandInfo.MinCommandLength:
                throw new Exception("Command name minimum length not reached");
            case > CommandInfo.MaxCommandLength:
                throw new Exception("Command name maximum length exceeded");
        }

        if (Callback == null)
            throw new NullReferenceException("Callback for command not set");

        Parameters.ForEach(pb =>
        {
            if (string.IsNullOrEmpty(pb.Name))
                throw new ArgumentNullException(nameof(ParameterBuilder.Name));

            if (Parameters.Any(other => !ReferenceEquals(other, pb) && other.Name == pb.Name))
                throw new Exception($"Duplicate parameter name `{pb.Name}` for command `{Name}`");
        });

        return new CommandInfo(this);
    }
}
