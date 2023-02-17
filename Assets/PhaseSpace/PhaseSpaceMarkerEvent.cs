using PhaseSpace.Unity;
using System;
using UnityEngine.Events;


/// <summary>
/// Represents a PhaseSpace marker event.
/// </summary>
[Serializable]
public class PhaseSpaceMarkerEvent : UnityEvent<PhaseSpaceMarkerEventArgs> { }


/// <summary>
/// Represents the arguments of the <see cref="PhaseSpaceMarkerEvent"/> event.
/// </summary>
public class PhaseSpaceMarkerEventArgs {

    /// <summary>
    /// Gets the associated marker data.
    /// </summary>
    public Marker Marker { get; }
    /// <summary>
    /// Gets the timestamp of the marker data.
    /// </summary>
    public string Timestamp { get; }


    /// <summary>
    /// Initializes a new <see cref="PhaseSpaceMarkerEventArgs"/>.
    /// </summary>
    /// <param name="timestamp">The timestamp of the marker data.</param>
    /// <param name="marker">The associated marker data.</param>
    public PhaseSpaceMarkerEventArgs(string timestamp, Marker marker) {
        this.Marker = marker;
        this.Timestamp = timestamp;
    }

}