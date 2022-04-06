using System.Collections.Generic;
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
    bool TryParse(string? input, IReadOnlyList<IParameterInfo> parameters,
        out CommandArgs? args, out ArgumentsParseError? error);
}
