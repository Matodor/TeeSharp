using System;

namespace TeeSharp.Core.Settings;

public interface ISettingsChangesNotifier<out TSettings>
{
    TSettings Current { get; }

    IDisposable? Subscribe(Action<TSettings> callback);
}
