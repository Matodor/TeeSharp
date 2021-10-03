// ReSharper disable StringLiteralTypo

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TeeSharp.Common.Commands;
using TeeSharp.Common.Commands.Builders;
using TeeSharp.Common.Commands.Errors;
using TeeSharp.Common.Commands.Parsers;

namespace TeeSharp.Tests
{
    public class CommandsTests
    {
        [OneTimeSetUp]
        public void Init()
        {
        }

        [Test]
        [TestCase("", new string[] {"?s"}, new object[] {})]
        [TestCase("test", new string[] {"s", "?s"}, new object[] {"test", null})]
        [TestCase("test", new string[] {"s"}, new object[] {"test"})]
        [TestCase("test test", new string[] {"s", "s"}, new object[] {"test", "test"})]
        public void ShouldParseArgumentsLine(string line, string[] paramsPatterns, object[] expectedArgs)
        {
            var parameters = paramsPatterns
                .Select(p => ParameterBuilder.FromPattern(p).Build())
                .ToArray();

            var parser = new DefaultCommandArgumentsParser();
            var result = parser.TryParse(line, parameters, out var args, out var error);
            
            Assert.True(result);
            Assert.AreEqual(new CommandArgs(expectedArgs), args);
            Assert.AreEqual(null, error);
        }

        [Test]
        [TestCase("//r", "r", null)]
        [TestCase("//r ", "r", null)]
        [TestCase(" //r ", "r", null)]
        [TestCase("//reg login pass", "reg", "login pass")]
        public void ShouldParseCommandLine(string line, string expectedCmd, string expectedArgs)
        {
            var parser = new DefaultCommandLineParser("//");
            var result = parser.TryParse(line, out var command, out var args, out var error);

            Assert.True(result);
            Assert.AreEqual(expectedCmd, command);
            Assert.AreEqual(expectedArgs, args);
            Assert.AreEqual(null, error);
        }
        
        [Test]
        [TestCase("", null, null, LineParseError.EmptyLine)]
        [TestCase("/", null, null, LineParseError.BadLength)]
        [TestCase("/a", null, null, LineParseError.BadLength)]
        [TestCase("//", null, null, LineParseError.BadLength)]
        [TestCase("// ", null, null, LineParseError.BadLength)]
        [TestCase("// a", null, null, LineParseError.BadLength)]
        [TestCase("/ a", null, null, LineParseError.WrongPrefix)]
        [TestCase("**a", null, null, LineParseError.WrongPrefix)]
        [TestCase("[[a", null, null, LineParseError.WrongPrefix)]
        public void ShouldParseCommandLineWithErrors(string line, string expectedCmd, 
            string expectedArgs, LineParseError expectedError)
        {
            var parser = new DefaultCommandLineParser("//");
            var result = parser.TryParse(line, out var command, out var args, out var error);

            Assert.False(result);
            Assert.AreEqual(expectedCmd, command);
            Assert.AreEqual(expectedArgs, args);
            Assert.AreEqual(expectedError, error);
        }
    }
}
