using PhaseSpace.Unity;
using System;
using UnityEngine.Events;


/// <summary>
/// Represents a PhaseSpace camera event.
/// </summary>
[Serializable]
public class PhaseSpaceCameraEvent : UnityEvent<PhaseSpaceCameraEventArgs> { }


/// <summary>
/// Represents the arguments of the <see cref="PhaseSpaceCameraEvent"/> event.
/// </summary>
public class PhaseSpaceCameraEventArgs {

    /// <summary>
    /// Gets the associated camera data.
    /// </summary>
    public Camera Camera { get; }
    /// <summary>
    /// Gets the timestamp of the marker data.
    /// </summary>
    public string Timestamp { get; }


    /// <summary>
    /// Initializes a new <see cref="PhaseSpaceCameraEventArgs"/>.
    /// </summary>
    /// <param name="timestamp">The timestamp of the camera data.</param>
    /// <param name="camera">The associated camera data.</param>
    public PhaseSpaceCameraEventArgs(string timestamp, Camera camera) {
        this.Camera = camera;
        this.Timestamp = timestamp;
    }

}