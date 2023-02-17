/// <summary>
/// Represents the interface for custom loggable types.
/// </summary>
public interface ILoggable {

    /// <summary>
    /// Converts the given value to a loggable representation.
    /// </summary>
    /// <returns>
    /// Returns a <see cref="string"/> representing the given value.
    /// </returns>
    string ToLogString();

}