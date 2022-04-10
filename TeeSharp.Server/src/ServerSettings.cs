using System;

namespace TeeSharp.Server;

public class ServerSettings
{
    public event Action<string> NameChanged = delegate {  };

    public bool UseHotReload { get; set; } = true;

    public string Name
    {
        get => _name;
        set => NameChanged.Invoke(_name = value);
    }

    private string _name = "[TeeSharp] Unnamed server";
}
