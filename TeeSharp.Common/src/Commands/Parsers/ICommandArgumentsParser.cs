namespace TeeSharp.Common.Commands.Parsers
{
    public interface ICommandArgumentsParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parametersPattern"></param>
        /// <param name="args"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryParse(string input, string parametersPattern, 
            out CommandArgs args, out ArgumentsParseError? error);
    }
}