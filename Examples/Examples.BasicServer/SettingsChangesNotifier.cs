using Microsoft.Extensions.Options;
using TeeSharp.Core;
using TeeSharp.Common.Settings;

namespace Examples.BasicServer;

public class SettingsChangesNotifier<TSettings> : ISettingsChangesNotifier<TSettings>
{
    public TSettings Current => _monitor.CurrentValue;

    private readonly IOptionsMonitor<TSettings> _monitor;

    public SettingsChangesNotifier(IOptionsMonitor<TSettings> monitor)
    {
        _monitor = monitor;
    }

    public IDisposable? Subscribe(Action<TSettings> callback)
    {
        return _monitor.OnChange(
            Debouncer.Debounce<TSettings, string?>(
                (settings, _) => callback(settings),
                TimeSpan.FromSeconds(1)
            )
        );
    }
}
