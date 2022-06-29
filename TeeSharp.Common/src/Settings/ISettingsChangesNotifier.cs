using System;

namespace TeeSharp.Common.Settings;

public interface ISettingsChangesNotifier<out TSettings>
{
    TSettings Current { get; }

    IDisposable? Subscribe(Action<TSettings> callback);
}
