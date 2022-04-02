// ReSharper disable StringLiteralTypo
// ReSharper disable RedundantExplicitArrayCreation

using System.Linq;
using NUnit.Framework;
using TeeSharp.Commands;
using TeeSharp.Commands.Builders;
using TeeSharp.Commands.Errors;
using TeeSharp.Commands.Parsers;

namespace TeeSharp.Tests
{
    public class CommandsTests
    {
        [OneTimeSetUp]
        public void Init()
        {
        }

        [Test]
        [TestCase("", new string[] {}, new object[] {})]
        [TestCase("\"", new string[] {}, new object[] {})]
        [TestCase("\"\"", new string[] {}, new object[] {})]
        [TestCase("   ", new string[] {}, new object[] {})]
        [TestCase("\"   \"", new string[] {"s"}, new object[] {"   "})]
        [TestCase("", new string[] {"?s"}, new object[] {})]
        [TestCase("   ", new string[] {"?s"}, new object[] {})]
        [TestCase("test", new string[] {"s"}, new object[] {"test"})]
        [TestCase("te\"st", new string[] {"s"}, new object[] {"te\"st"})]
        [TestCase("\"te\"st\"", new string[] {"s"}, new object[] {"te\"st"})]
        // "test"
        [TestCase("\"test\"", new string[] {"s"}, new object[] {"test"})]
        // \"test\"
        [TestCase("\\\"test\\\"", new string[] {"s"}, new object[] {"\"test\""})]
        // "\"test"
        [TestCase("\"\\\"test\"", new string[] {"s"}, new object[] {"\"test"})]
        // \"t\"e\"s\"t\"
        [TestCase("\\\"t\\\"e\\\"s\\\"t\\\"", new string[] {"s"}, new object[] {"\"t\"e\"s\"t\""})]
        // "\" t \" e \" s \" t \""
        [TestCase("\"\\\" t \\\" e \\\" s \\\" t \\\"\"", new string[] {"s"}, new object[] {"\" t \" e \" s \" t \""})]
        [TestCase("\"test\" test", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase("test \"test\"", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase("  test \"test\"", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" test  \"test\" ", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" \"test\" \"test\" ", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" \"test\" \"test\" ", new string[] {"?s", "?s"}, new object[] {"test", "test"})]
        [TestCase("test \"test\"", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase("\"test test\"", new string[] {"s"}, new object[] {"test test"})]
        [TestCase("  test", new string[] {"s"}, new object[] {"test"})]
        [TestCase(" test ", new string[] {"s"}, new object[] {"test"})]
        [TestCase("test  ", new string[] {"s"}, new object[] {"test"})]
        [TestCase("test", new string[] {"s", "?s"}, new object[] {"test"})]
        [TestCase("  test", new string[] {"s", "?s"}, new object[] {"test"})]
        [TestCase(" test ", new string[] {"s", "?s"}, new object[] {"test"})]
        [TestCase("test  ", new string[] {"s", "?s"}, new object[] {"test"})]
        [TestCase("\\\" \\\"", new string[] {"s", "s"}, new object[] {"\"", "\""})]
        [TestCase("test test", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" test test", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" test  test", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" test  test ", new string[] {"s", "s"}, new object[] {"test", "test"})]
        [TestCase(" test , test ", new string[] {"s", "s", "s"}, new object[] {"test", ",", "test"})]
        public void ShouldParseStringArgumentsLine(string line, string[] paramsPatterns, object[] expectedArgs)
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
