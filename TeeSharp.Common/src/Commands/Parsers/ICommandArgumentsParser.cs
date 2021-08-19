using System;
using System.Collections.Generic;

namespace TeeSharp.Common.Commands.Parsers
{
    public interface ICommandArgumentsParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parametersPattern"></param>
        /// <returns></returns>
        bool TryParse(string input, string parametersPattern, 
            out CommandArgs args);
    }
}