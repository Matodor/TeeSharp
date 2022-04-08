using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Commands.Errors;

namespace TeeSharp.Commands;

public interface ICommandArgumentsParser
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <param name="parameters"></param>
    /// <param name="args"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    bool TryParse(
        ReadOnlySpan<char> input,
        IReadOnlyList<IParameterInfo> parameters,
        out CommandArgs args,
        [NotNullWhen(false)] out ArgumentsParseError? error);
}
