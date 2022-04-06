using System;
using System.Collections.Generic;
using TeeSharp.Commands.Builders;

namespace TeeSharp.Commands;

public interface ICommandsDictionary : IDictionary<string, CommandInfo>
{
    event Action<string, CommandInfo>? CommandAdded;
    event Action<string>? CommandRemoved;

    void Init();
    void Add(Action<CommandBuilder> factory);
}
