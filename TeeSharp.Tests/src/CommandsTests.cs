// ReSharper disable StringLiteralTypo
// ReSharper disable RedundantExplicitArrayCreation

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TeeSharp.Commands;
using TeeSharp.Commands.ArgumentReaders;
using TeeSharp.Commands.Builders;
using TeeSharp.Commands.Errors;
using TeeSharp.Commands.Parsers;
using TeeSharp.Core;

namespace TeeSharp.Tests;

public class CommandsTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Tee.Logger = NullLogger.Instance;
        Tee.LoggerFactory = NullLoggerFactory.Instance;
    }

    [Test]
    public void ShouldExecuteCommandWithParams()
    {
        var sum = 0f;
        var executor = new CommandsExecutor();

        executor.Commands.Add(builder =>
        {
            builder
                .WithName("sum")
                .WithParam("i", parameterBuilder =>
                {
                    parameterBuilder.WithName("x");
                })
                .WithParam("f", parameterBuilder =>
                {
                    parameterBuilder.WithName("y");
                })
                .WithParam("?s", parameterBuilder =>
                {
                    parameterBuilder.WithName("z");
                })
                .WithCallback((args, _, _) =>
                {
                    sum += (int)args["x"];
                    sum += (float)args["y"];
                    return Task.CompletedTask;
                });
        });

        var result = executor.Execute("/sum 10 656.99", CommandContext.Default, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.True(result.Args.ContainsKey("x"));
        Assert.True(result.Args.ContainsKey("y"));
        Assert.False(result.Args.ContainsKey("z"));

        Assert.Null(result.LineParseError);
        Assert.Null(result.ArgumentsParseError);
        Assert.Null(result.Error);
        Assert.AreEqual(sum, 666.99f);
        Assert.AreEqual(result.Args["x"], 10);
        Assert.AreEqual(result.Args["y"], 656.99f);
    }

    [Test]
    public void ShouldExecuteCommand()
    {
        var sum = 0;
        var executor = new CommandsExecutor();

        executor.Commands.Add(builder =>
        {
            builder
                .WithName("add")
                .WithCallback((_, _, _) =>
                {
                    sum += 1;
                    sum += 1;
                    sum += 1;
                    return Task.CompletedTask;
                });
        });

        var result = executor.Execute("/add", CommandContext.Default, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Null(result.LineParseError);
        Assert.Null(result.ArgumentsParseError);
        Assert.Null(result.Error);
        Assert.AreEqual(result.Args, CommandArgs.Empty);
        Assert.AreEqual(sum, 3);
    }

    [Test]
    public void ShouldContainsCommand()
    {
        var dictionary = new CommandsDictionary()
        {
            builder =>
            {
                builder
                    .WithName("test")
                    .WithCallback((_, _, _) => Task.CompletedTask);
            },
            builder =>
            {
                builder
                    .WithName("r")
                    .WithCallback((_, _, _) => Task.CompletedTask);
            },
        };

        Assert.True(dictionary.ContainsKey("test"));
    }

    [Test]
    [TestCase("a", "a")]
    [TestCase("b", "b")]
    [TestCase("c", "c")]
    public void StringReaderShouldParseArgument(string arg, string expected)
    {
        var argumentReader = new StringReader();
        var result = argumentReader.TryRead(arg, out var value);

        Assert.True(result);
        Assert.AreEqual(value, expected);
    }

    [Test]
    [TestCase("1", 1)]
    [TestCase("2", 2)]
    [TestCase("123", 123)]
    [TestCase("2147483647", 2147483647)]
    [TestCase("2,147,483,647", 2147483647)]
    public void IntReaderShouldParseArgument(string arg, int expected)
    {
        var argumentReader = new IntReader();
        var result = argumentReader.TryRead(arg, out var value);

        Assert.True(result);
        Assert.AreEqual(value, expected);
    }

    [Test]
    [TestCase("a")]
    [TestCase("1 1")]
    [TestCase("11a")]
    [TestCase("a11")]
    [TestCase("11,")]
    [TestCase("11,,")]
    [TestCase(",11")]
    [TestCase("11.")]
    [TestCase(".11")]
    [TestCase("3.5")]
    [TestCase(".5")]
    [TestCase("1/")]
    [TestCase("214748364123")]
    public void IntReaderShouldNotParseArgument(string arg)
    {
        var argumentReader = new IntReader();
        var result = argumentReader.TryRead(arg, out _);

        Assert.False(result);
    }

    [Test]
    [TestCase("1", 1)]
    [TestCase("1.5", 1.5f)]
    [TestCase("2.123123", 2.123123f)]
    [TestCase("2.999999", 2.999999f)]
    [TestCase("123", 123)]
    [TestCase("123,456", 123456)]
    [TestCase("123,456.99", 123456.99f)]
    [TestCase("340282300000000000000000000000000000000", 340282300000000000000000000000000000000f)]
    public void FloatReaderShouldParseArgument(string arg, float expected)
    {
        var argumentReader = new FloatReader();
        var result = argumentReader.TryRead(arg, out var value);

        Assert.True(result);
        Assert.AreEqual(value, expected);
    }

    [Test]
    [TestCase("a")]
    [TestCase("1 1")]
    [TestCase("3 .5")]
    [TestCase(".5")]
    [TestCase("5.")]
    [TestCase("5,")]
    [TestCase(",5.")]
    [TestCase("5.12.123")]
    [TestCase("5.12,213")]
    [TestCase("1/")]
    public void FloatReaderShouldNotParseArgument(string arg)
    {
        var argumentReader = new FloatReader();
        var result = argumentReader.TryRead(arg, out _);

        Assert.False(result);
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
            .Select((p, i) => ParameterBuilder.FromPattern(p).WithName(i.ToString()).Build())
            .ToArray();

        var parser = new DefaultCommandArgumentsParser();
        var result = parser.TryParse(line, parameters, out var args, out var error);

        Assert.True(result);
        Assert.AreEqual(
            new CommandArgs(
                expectedArgs
                    .Select((arg, idx) => (arg, idx: idx.ToString()))
                    .ToDictionary(t => t.idx, t => t.arg)
            ),
            args
        );
        Assert.AreEqual(null, error);
    }

    [Test]
    [TestCase("", new string[] {}, new object[] {})]
    [TestCase("   ", new string[] {"?i"}, new object[] {})]
    [TestCase("123", new string[] {"i"}, new object[] {123})]
    [TestCase("\"123\"", new string[] {"i"}, new object[] {123})]
    [TestCase("\"123\" 456", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase("123 \"456\"", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase("  123 \"456\"", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" 123  \"456\" ", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" \"123\" \"456\" ", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" \"123\" \"456\" ", new string[] {"?i", "?i"}, new object[] {123, 456})]
    [TestCase("123 \"456\"", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase("  123", new string[] {"i"}, new object[] {123})]
    [TestCase(" 123 ", new string[] {"i"}, new object[] {123})]
    [TestCase("123  ", new string[] {"i"}, new object[] {123})]
    [TestCase("123", new string[] {"i", "?i"}, new object[] {123})]
    [TestCase("  123", new string[] {"i", "?i"}, new object[] {123})]
    [TestCase(" 123 ", new string[] {"i", "?i"}, new object[] {123})]
    [TestCase("123  ", new string[] {"i", "?i"}, new object[] {123})]
    [TestCase("123 456", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" 123 456", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" 123  456", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" 123  456 ", new string[] {"i", "i"}, new object[] {123, 456})]
    [TestCase(" 123 , 456 ", new string[] {"i", "s", "i"}, new object[] {123, ",", 456})]
    public void ShouldParseIntArgumentsLine(string line, string[] paramsPatterns, object[] expectedArgs)
    {
        var parameters = paramsPatterns
            .Select((p, i) => ParameterBuilder.FromPattern(p).WithName(i.ToString()).Build())
            .ToArray();

        var parser = new DefaultCommandArgumentsParser();
        var result = parser.TryParse(line, parameters, out var args, out var error);

        Assert.True(result);
        Assert.AreEqual(
            new CommandArgs(
                expectedArgs
                    .Select((arg, idx) => (arg, idx: idx.ToString()))
                    .ToDictionary(t => t.idx, t => t.arg)
            ),
            args
        );
        Assert.AreEqual(null, error);
    }

    [Test]
    [TestCase("", new string[] {}, new object[] {})]
    [TestCase("   ", new string[] {"?f"}, new object[] {})]
    [TestCase("123.777", new string[] {"f"}, new object[] {123.777f})]
    [TestCase("\"123.777\"", new string[] {"f"}, new object[] {123.777f})]
    [TestCase("\"123.777\" 456.99", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase("123.777 \"456.99\"", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase("  123.777 \"456.99\"", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" 123.777  \"456.99\" ", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" \"123.777\" \"456.99\" ", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" \"123.777\" \"456.99\" ", new string[] {"?f", "?f"}, new object[] {123.777f, 456.99f})]
    [TestCase("123.777 \"456.99\"", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase("  123.777", new string[] {"f"}, new object[] {123.777f})]
    [TestCase(" 123.777 ", new string[] {"f"}, new object[] {123.777f})]
    [TestCase("123.777  ", new string[] {"f"}, new object[] {123.777f})]
    [TestCase("123.777", new string[] {"f", "?f"}, new object[] {123.777f})]
    [TestCase("  123.777", new string[] {"f", "?f"}, new object[] {123.777f})]
    [TestCase(" 123.777 ", new string[] {"f", "?f"}, new object[] {123.777f})]
    [TestCase("123.777  ", new string[] {"f", "?f"}, new object[] {123.777f})]
    [TestCase("123.777 456.99", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" 123.777 456.99", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" 123.777  456.99", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" 123.777  456.99 ", new string[] {"f", "f"}, new object[] {123.777f, 456.99f})]
    [TestCase(" 123.777 , 456.99 ", new string[] {"f", "s", "f"}, new object[] {123.777f, ",", 456.99f})]
    public void ShouldParseFloatArgumentsLine(string line, string[] paramsPatterns, object[] expectedArgs)
    {
        var parameters = paramsPatterns
            .Select((p, i) => ParameterBuilder.FromPattern(p).WithName(i.ToString()).Build())
            .ToArray();

        var parser = new DefaultCommandArgumentsParser();
        var result = parser.TryParse(line, parameters, out var args, out var error);

        Assert.True(result);
        Assert.AreEqual(
            new CommandArgs(
                expectedArgs
                    .Select((arg, idx) => (arg, idx: idx.ToString()))
                    .ToDictionary(t => t.idx, t => t.arg)
            ),
            args
        );
        Assert.AreEqual(null, error);
    }

    [Test]
    [TestCase("//r", "r", null)]
    [TestCase("//r ", "r", null)]
    [TestCase(" //r ", "r", null)]
    [TestCase("//reg login pass", "reg", "login pass")]
    public void ShouldParseCommandNameFromLine(string line, string? expectedCmd, string? expectedArgs)
    {
        var parser = new DefaultCommandLineParser("//");
        var result = parser.TryParse(line, out var command, out var args, out var error);

        Assert.True(result);
        Assert.AreEqual(expectedCmd.AsSpan().ToString(), command.ToString());
        Assert.AreEqual(expectedArgs.AsSpan().ToString(), args.ToString());
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
    public void ShouldParseCommandLineWithErrors(
        string line,
        string? expectedCmd,
        string? expectedArgs,
        LineParseError expectedError)
    {
        var parser = new DefaultCommandLineParser("//");
        var result = parser.TryParse(line, out var command, out var args, out var error);

        Assert.False(result);
        Assert.AreEqual(expectedCmd.AsSpan().ToString(), command.ToString());
        Assert.AreEqual(expectedArgs.AsSpan().ToString(), args.ToString());
        Assert.AreEqual(expectedError, error);
    }
}
