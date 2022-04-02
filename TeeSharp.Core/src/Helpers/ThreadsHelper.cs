using System.Runtime.InteropServices;
using System.Threading;

namespace TeeSharp.Core.Helpers;

public static class ThreadsHelper
{
#if _WINDOWS
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int NtQueryTimerResolution(
        out uint minimumResolution,
        out uint maximumResolution,
        out uint currentResolution);

    private static readonly double _lowestSleepThreshold;
#endif
        
    static ThreadsHelper()
    {
#if _WINDOWS
        NtQueryTimerResolution(out _, out var max, out _);
        _lowestSleepThreshold = 1.0 + (max / 10000.0);
#endif
    }

#if _WINDOWS
    /// <summary>
    /// Returns the current timer resolution in milliseconds
    /// </summary>
    private static double GetCurrentResolution()
    {
        NtQueryTimerResolution(out _, out _, out var current);
        return current / 10000.0;
    }

    /// <summary>
    /// Sleeps as long as possible without exceeding the specified period
    /// </summary>
    public static void SleepForNoMoreThan(double milliseconds)
    {
        // Assumption is that Thread.Sleep(t) will sleep for at least (t), and at most (t + timerResolution)
        if (milliseconds < _lowestSleepThreshold)
            return;

        var sleepTime = (int) (milliseconds - GetCurrentResolution());
        if (sleepTime > 0)
            Thread.Sleep(sleepTime);
    }
#endif
}