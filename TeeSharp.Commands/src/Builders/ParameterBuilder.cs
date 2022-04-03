using System;
using TeeSharp.Commands.ArgumentsReaders;

namespace TeeSharp.Commands.Builders;

public class ParameterBuilder : IParameterInfo
{
    // ReSharper disable once MemberCanBePrivate.Global
    public const char ParameterString = 's';
    // ReSharper disable once MemberCanBePrivate.Global
    public const char ParameterFloat = 'f';
    // ReSharper disable once MemberCanBePrivate.Global
    public const char ParameterInt = 'i';
        
    // ReSharper disable once MemberCanBePrivate.Global
    public const char ParameterRemain = 'r';
    // ReSharper disable once MemberCanBePrivate.Global
    public const char ParameterOptional = '?';
        
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsOptional { get; protected set; } = false;
    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsRemain { get; protected set; }
    // ReSharper disable once MemberCanBePrivate.Global
    public IArgumentReader ArgumentReader { get; protected set; } = null!;

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
        
    public ParameterInfo Build()
    {
        return new ParameterInfo(this);
    }
}
