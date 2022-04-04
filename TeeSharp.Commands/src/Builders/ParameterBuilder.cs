using System;
using TeeSharp.Commands.ArgumentsReaders;

namespace TeeSharp.Commands.Builders;

public class ParameterBuilder
{
    public const char ParameterString = 's';
    public const char ParameterFloat = 'f';
    public const char ParameterInt = 'i';

    public const char ParameterRemain = 'r';
    public const char ParameterOptional = '?';

    internal string? Name { get; set; }
    internal string? Description { get; set; }
    internal bool IsOptional { get; set; }
    internal bool IsRemain { get; set; }
    internal IArgumentReader ArgumentReader { get; set; } = null!;

    public static ParameterBuilder FromPattern(string pattern)
    {
        var builder = new ParameterBuilder();
        char type;

        switch (pattern.Length)
        {
            case 2:
                switch (pattern[0])
                {
                    case ParameterOptional:
                        builder.WithOptional(true);
                        break;
                    case ParameterRemain:
                        builder.WithRemain(true);
                        break;
                    default:
                        throw new Exception("ParameterBuilder: FromPattern optional parameter wrong format");
                }

                type = pattern[1];
                break;
            case 1:
                type = pattern[0];
                break;
            default:
                throw new ArgumentException($"Wrong {nameof(pattern)} length");
        }

        switch (type)
        {
            case ParameterString:
                builder.WithReader<StringReader>();
                break;

            case ParameterFloat:
                builder.WithReader<FloatReader>();
                break;

            case ParameterInt:
                builder.WithReader<IntReader>();
                break;

            default:
                throw new ArgumentException("Wrong parameter pattern type");
        }

        return builder;
    }

    public ParameterBuilder WithReader(Func<ParameterBuilder, IArgumentReader> factory)
    {
        ArgumentReader = factory(this);
        return this;
    }

    public ParameterBuilder WithReader<TReader>() where TReader : class, IArgumentReader, new()
    {
        ArgumentReader = new TReader();
        return this;
    }

    public ParameterBuilder WithName(string name)
    {
        Name = name.Trim();
        return this;
    }

    public ParameterBuilder WithDescription(string desc)
    {
        Description = desc.Trim();
        return this;
    }

    public ParameterBuilder WithOptional(bool isOptional)
    {
        IsOptional = isOptional;
        return this;
    }

    public ParameterBuilder WithRemain(bool isRemain)
    {
        IsRemain = isRemain;
        return this;
    }

    public IParameterInfo Build()
    {
        if (string.IsNullOrEmpty(Name))
            throw new NullReferenceException(nameof(Name));

        return new ParameterInfo(this);
    }
}
