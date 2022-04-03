using System;
using System.Globalization;

namespace TeeSharp.Commands.ArgumentsReaders;

public class FloatReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value)
    {
        if (!arg.IsEmpty && (!char.IsDigit(arg[0]) || !char.IsDigit(arg[^1])))
        {
            value = default(float);
            return false;
        }
        
        try
        {
            value = float.Parse(arg,
                NumberStyles.Float | NumberStyles.AllowThousands,
                NumberFormatInfo.InvariantInfo);
            return true;
        }
        catch (Exception)
        {
            value = default(float);
            return false;
        }
    }
}
