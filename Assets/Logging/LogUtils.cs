using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;


/// <summary>
/// Provides extension and utility methods for logging.
/// </summary>
public static class LogUtils {

    /// <summary>
    /// Represents the log culture used by all logging interfaces by default.
    /// </summary>
    public static readonly CultureInfo LogCulture = CultureInfo.InvariantCulture;
    /// <summary>
    /// Represents the global path of all log output.
    /// </summary>
    public const string LogPath = "Logs";


    /// <summary>
    /// Generates a new file name for a log file.
    /// </summary>
    /// <param name="baseName">Optional. The base name of the log file.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the generated log file name.
    /// </returns>
    public static string GenerateLogFileName(string baseName = "") {
        var builder = new StringBuilder();
        builder.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        if ( !string.IsNullOrEmpty(baseName) ) builder.Append($"_{baseName}");
        //if ( !string.IsNullOrEmpty(App.Instance.Configuration.id) ) builder.Append($"_{App.Instance.Configuration.id}");
        //if ( !string.IsNullOrEmpty(App.Instance.Configuration.tag) ) builder.Append($"_{App.Instance.Configuration.tag}");
        builder.Append(".csv");

        return Path.Combine(LogPath, builder.ToString());
    }


    /// <summary>
    /// Gets the current timestamp in a loggable format.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="string"/> representing the current timestamp.
    /// </returns>
    public static string GetTimestamp() => HighPrecisionClock.GetTimestamp().ToLogString();

    /// <summary>
    /// Gets a loggable representation of a <see langword="null"/> value.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="string"/> representing a <see langword="null"/> value.
    /// </returns>
    public static string GetNone() => "Unknown";


    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this bool value) => value ? "Yes" : "No";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this byte value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this sbyte value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this char value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this short value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this ushort value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this int value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this uint value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this long value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this ulong value) => value.ToString(LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this float value, int digits = 5) => value.ToString($"F{digits}", LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this double value, int digits = 8) => value.ToString($"F{digits}", LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Enum value) => value.ToString();

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this DateTime value) => value.ToString("yyyy.MM.dd hh:mm.ss.fff", LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this DateTimeOffset value) => value.ToString("yyyy.MM.dd hh:mm.ss.fff", LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this TimeSpan value) => value.ToString("c", LogCulture);

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Vector2 value, int digits = 5) => $"{value.x.ToLogString(digits)},{value.y.ToLogString(digits)}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Vector3 value, int digits = 5) => $"{value.x.ToLogString(digits)},{value.y.ToLogString(digits)},{value.z.ToLogString(digits)}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Vector4 value, int digits = 5) => $"{value.x.ToLogString(digits)},{value.y.ToLogString(digits)},{value.z.ToLogString(digits)},{value.w.ToLogString(digits)}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Vector2Int value) => $"{value.x.ToLogString()},{value.y.ToLogString()}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Vector3Int value) => $"{value.x.ToLogString()},{value.y.ToLogString()},{value.z.ToLogString()}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Quaternion value, int digits = 5) => $"{value.x.ToLogString(digits)},{value.y.ToLogString(digits)},{value.z.ToLogString(digits)},{value.w.ToLogString(digits)}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <param name="digits">Optional. The number of digits to display.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this Bounds value, int digits = 5) => $"{value.min.ToLogString(digits)},{value.max.ToLogString(digits)}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this BoundsInt value) => $"{value.min.ToLogString()},{value.max.ToLogString()}";

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <param name="value">The value to log.</param>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    public static string ToLogString(this object value) {
        if ( value is null ) return "";
        if ( value is string @string ) return @string;

        switch ( value ) {
            case bool e: return e.ToLogString();
            case byte e: return e.ToLogString();
            case sbyte e: return e.ToLogString();
            case char e: return e.ToLogString();
            case short e: return e.ToLogString();
            case ushort e: return e.ToLogString();
            case int e: return e.ToLogString();
            case uint e: return e.ToLogString();
            case long e: return e.ToLogString();
            case ulong e: return e.ToLogString();
            case float e: return e.ToLogString();
            case double e: return e.ToLogString();
            case Enum e: return e.ToLogString();
            case DateTime e: return e.ToLogString();
            case DateTimeOffset e: return e.ToLogString();
            case TimeSpan e: return e.ToLogString();
            case Vector2 e: return e.ToLogString();
            case Vector3 e: return e.ToLogString();
            case Vector4 e: return e.ToLogString();
            case Vector2Int e: return e.ToLogString();
            case Vector3Int e: return e.ToLogString();
            case Quaternion e: return e.ToLogString();
            case Bounds e: return e.ToLogString();
            case BoundsInt e: return e.ToLogString();
            case ILoggable e: return e.ToLogString();
            default: return value.ToString();
        }
    }

}