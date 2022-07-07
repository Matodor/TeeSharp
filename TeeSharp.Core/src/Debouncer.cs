using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeeSharp.Core;

public static class Debouncer
{
    public static Action<T> Debounce<T>(Action<T> action, TimeSpan delay)
    {
        CancellationTokenSource? cts = null;

        return parameter =>
        {
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            cts = new CancellationTokenSource();
            Task.Delay(delay, cts.Token)
                .ContinueWith(_ => action(parameter), cts.Token)
                .ContinueWith(_ => cts.Dispose(), cts.Token);
        };
    }

    public static Action<T1, T2> Debounce<T1, T2>(Action<T1, T2> action, TimeSpan delay)
    {
        CancellationTokenSource? cts = null;

        return (p1, p2) =>
        {
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            cts = new CancellationTokenSource();
            Task.Delay(delay, cts.Token)
                .ContinueWith(_ => action(p1, p2), cts.Token)
                .ContinueWith(_ => cts.Dispose(), cts.Token);
        };
    }

    public static Action<T1, T2, T3> Debounce<T1, T2, T3>(Action<T1, T2, T3> action, TimeSpan delay)
    {
        CancellationTokenSource? cts = null;

        return (p1, p2, p3) =>
        {
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }

            cts = new CancellationTokenSource();
            Task.Delay(delay, cts.Token)
                .ContinueWith(_ => action(p1, p2, p3), cts.Token)
                .ContinueWith(_ => cts.Dispose(), cts.Token);
        };
    }
}
