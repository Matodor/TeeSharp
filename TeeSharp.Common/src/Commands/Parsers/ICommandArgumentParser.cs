using System;
using System.Collections.Generic;

namespace TeeSharp.Common.Commands.Parsers
{
    public interface ICommandArgumentParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        IEnumerable<object> Parse(string input, string pattern);
    }
}