using System;
using System.Collections.Generic;
using System.Linq;

namespace TeeSharp.Commands.Builders;

public class CommandBuilder
{
    internal List<ParameterBuilder> Parameters { get; set; }
    internal CommandHandler Callback { get; set; } = null!;
    internal string? Name { get; set; }
    internal string? Description { get; set; }

    public CommandBuilder()
    {
        Parameters = new List<ParameterBuilder>();
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
