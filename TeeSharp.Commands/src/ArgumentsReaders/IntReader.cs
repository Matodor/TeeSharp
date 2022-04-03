using System;
using System.Globalization;

namespace TeeSharp.Commands.ArgumentsReaders;

public class IntReader : IArgumentReader
{
    public bool TryRead(ReadOnlySpan<char> arg, out object value)
    {
        if (!arg.IsEmpty && (!char.IsDigit(arg[0]) || !char.IsDigit(arg[^1])))
        {
            value = default(int);
            return false;
        }
        
        try
        {
            value = int.Parse(arg,
                NumberStyles.Integer | NumberStyles.AllowThousands,
                NumberFormatInfo.InvariantInfo);
            return true;
        }
        catch (Exception)
        {
            value = default(int);
            return false;
        }
    }
}
