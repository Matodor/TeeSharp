using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TeeSharp.Common.Commands;
using TeeSharp.Common.Commands.Parsers;

namespace TeeSharp.Tests
{
    public class CommandTests
    {
        private CommandsDictionary _store;
        [OneTimeSetUp]
        public void Init()
        {
            _store = new CommandsDictionary();
            _store.Dictionary.Add("rcon", new Command("rcon", string.Empty, "Rcon command"));
            _store.Dictionary.Add("test", new Command("test", "ii", "Test command"));
        }
        [Test]
        [TestCase("kotc9 test", "ss", new object[]{"kotc9", "test"})]
        [TestCase("kotc 9", "si", new object[]{"kotc", 9})]
        [TestCase("kotc9 rewr", "ss?s", new object[]{"kotc9", "rewr"})]
        [TestCase("kotc 9 jk jk fd", "sir", new object[]{"kotc", 9, "jk jk fd"})]
        [TestCase("kotc \"9 jk jk fd\"", "ss", new object[]{"kotc", "9 jk jk fd"})]
        [TestCase("\"9 jk jk f", "r", new object[]{"9 jk jk fd"})]
        public void ShouldParseArgumentCommand_DefaultParser(string data, string pattern, IEnumerable<object> excepted)
        {
            var parser = new DefaultCommandArgumentParser();

            var result = parser.Parse(data, pattern)?.ToList();

            var exceptedList = excepted.ToList();
            
            Assert.NotNull(result);
            Assert.AreEqual(result.Count, exceptedList.Count());

            for(var i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i], exceptedList[i]);
            }
        }
        
        [Test]
        [TestCase("kotc9", "ss")]
        [TestCase("kotc 9", "s")]
        [TestCase("kotc9", "i")]
        [TestCase("kotc 9 jk jk fd", "sri")]
        public void ShouldNotParseArgumentCommand_DefaultParser_WithInvalidData(string data, string pattern)
        {
            var parser = new DefaultCommandArgumentParser();

            var result = parser.Parse(data, pattern);
            
            Assert.Null(result);
        }

        [Test]
        [TestCase("rcon", "rcon")]
        [TestCase("test fdsf fdsfsd", "test")]
        public void ShouldParseCommand_DefaultParser(string data, string excepted)
        {
            var parser = new DefaultCommandParser();
            var (ok, command, _) = _store.Parse(data);
            
            Assert.True(ok);
            Assert.AreEqual(excepted, command.Cmd);
        }
    }
}