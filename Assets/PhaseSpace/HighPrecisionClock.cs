using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;


/// <summary>
/// Represents a clock that allows to receive high-precision timestamps.
/// </summary>
public static class HighPrecisionClock {

    private static readonly long MaxIdleTime = TimeSpan.FromSeconds(10).Ticks;

    [ThreadStatic] private static DateTime startTime;
    [ThreadStatic] private static double startTimestamp;


    /// <summary>
    /// Gets whether the clock supports high-precision timestamps.
    /// </summary>
    public static bool IsHighPrecision { get; private set; }



    /// <summary>
    /// Initializes the high-precision clock.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    public static void Initialize() {
        startTime = DateTime.UtcNow;
        startTimestamp = Stopwatch.GetTimestamp();

        try {
            GetSystemTimePreciseAsFileTime(out _);
            IsHighPrecision = true;
        }
        catch ( EntryPointNotFoundException ) {
            IsHighPrecision = false;
        }
    }



    /// <summary>
    /// Gets the current high-precision timestamp of the system.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="DateTime"/> representing the current high-precision timestamp of the underlying system.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetTimestamp() {
        if ( IsHighPrecision ) {
            GetSystemTimePreciseAsFileTime(out var preciseTime);
            return DateTime.FromFileTimeUtc(preciseTime);
        }

        var endTimestamp = Stopwatch.GetTimestamp();
        var duration = ( endTimestamp - startTimestamp ) / Stopwatch.Frequency * TimeSpan.TicksPerSecond;
        if ( duration >= MaxIdleTime ) {
            startTimestamp = Stopwatch.GetTimestamp();
            startTime = DateTime.UtcNow;
            return startTime;
        }

        return startTime.AddTicks(( long ) duration);
    }



    /// <summary>
    /// Gets the high-precision time of the OS used for file timing.
    /// </summary>
    /// <param name="filetime">[Out] The current high-precision file time.</param>
    [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
    private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

}