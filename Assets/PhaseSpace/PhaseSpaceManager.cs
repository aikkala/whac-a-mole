using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Camera = PhaseSpace.Unity.Camera;
using Marker = PhaseSpace.Unity.Marker;


/// <summary>
/// Represents the manager for the PhaseSpace module.
/// </summary>
public class PhaseSpaceManager : MonoBehaviour {

    [Header("Events")]
    [SerializeField] private PhaseSpaceCameraEvent onCameraFrame = new PhaseSpaceCameraEvent();
    [SerializeField] private PhaseSpaceMarkerEvent onMarkerFrame = new PhaseSpaceMarkerEvent();

    private readonly Dictionary<uint, Camera> latestCameraData = new Dictionary<uint, Camera>();
    private readonly Dictionary<uint, Marker> latestMarkerData = new Dictionary<uint, Marker>();


    /// <summary>
    /// Gets the latest available camera data.
    /// </summary>
    public ReadOnlyDictionary<uint, Camera> LatestCameraData => new ReadOnlyDictionary<uint, Camera>(this.latestCameraData);
    /// <summary>
    /// Gets the latest available marker data.
    /// </summary>
    public ReadOnlyDictionary<uint, Marker> LatestMarkerData => new ReadOnlyDictionary<uint, Marker>(this.latestMarkerData);

    /// <summary>
    /// Gets the event that is raised when new camera information has been gathered.
    /// </summary>
    public PhaseSpaceCameraEvent OnCameraFrame => this.onCameraFrame;
    /// <summary>
    /// Gets the event that is raised when new marker information has been gathered.
    /// </summary>
    public PhaseSpaceMarkerEvent OnMarkerFrame => this.onMarkerFrame;



    /// <summary>
    /// Gets the latest state of the given camera.
    /// </summary>
    /// <param name="cameraId">The id of the camera to get the latest state of.</param>
    /// <returns>
    /// Returns a <see cref="Camera"/> representing the latest state of the given camera.
    /// </returns>
    public Camera GetCamera(uint cameraId) => this.latestCameraData.TryGetValue(cameraId, out var camera) ? camera : this.latestCameraData[cameraId] = camera;

    /// <summary>
    /// Gets the latest state of the given marker.
    /// </summary>
    /// <param name="markerId">The id of the marker to get the latest state of.</param>
    /// <returns>
    /// Returns a <see cref="Marker"/> representing the latest state of the given marker.
    /// </returns>
    public Marker GetMarker(uint markerId) => this.latestMarkerData.TryGetValue(markerId, out var marker) ? marker : this.latestMarkerData[markerId] = marker;



    /// <summary>
    /// Reports a new camera data frame.
    /// </summary>
    /// <param name="timestamp">The timestamp of the camera data.</param>
    /// <param name="camera">The associated camera data.</param>
    public void ReportCameraFrame(string timestamp, Camera camera) {
        this.latestCameraData[camera.id] = camera;
        this.onCameraFrame.Invoke(new PhaseSpaceCameraEventArgs(timestamp, camera));
    }

    /// <summary>
    /// Reports a new marker data frame.
    /// </summary>
    /// <param name="timestamp">The timestamp of the marker data.</param>
    /// <param name="marker">The associated marker data.</param>
    public void ReportMarkerFrame(string timestamp, Marker marker) {
        this.latestMarkerData[marker.id] = marker;
        this.onMarkerFrame.Invoke(new PhaseSpaceMarkerEventArgs(timestamp, marker));
    }

}