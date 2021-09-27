// ReSharper disable StringLiteralTypo

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TeeSharp.Common.Commands;
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
        
        // [Test]
        // [TestCase("kotc9 test", "ss", new object[]{"kotc9", "test"})]
        // [TestCase("kotc 9", "si", new object[]{"kotc", 9})]
        // [TestCase("kotc9 rewr", "ss?s", new object[]{"kotc9", "rewr"})]
        // [TestCase("kotc 9 jk jk fd", "sir", new object[]{"kotc", 9, "jk jk fd"})]
        // [TestCase("kotc \"9 jk jk fd\"", "ss", new object[]{"kotc", "9 jk jk fd"})]
        // [TestCase("\"9 jk jk f", "r", new object[]{"9 jk jk fd"})]
        // public void ShouldParseArgumentCommand_DefaultParser(string line, string pattern, IEnumerable<object> excepted)
        // {
        //     // var parser = new DefaultCommandArgumentsParser();
        //     // var result = parser.TryParse()
        //     //
        //     // var exceptedList = excepted.ToList();
        //     //
        //     // Assert.NotNull(result);
        //     // Assert.AreEqual(result.Count, exceptedList.Count());
        //     //
        //     // for(var i = 0; i < result.Count; i++)
        //     // {
        //     //     Assert.AreEqual(result[i], exceptedList[i]);
        //     // }
        // }

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