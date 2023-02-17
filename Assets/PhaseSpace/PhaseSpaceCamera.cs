using UnityEngine;


/// <summary>
/// Represents a component to visualize a PhaseSpace camera.
/// </summary>
public class PhaseSpaceCamera : MonoBehaviour {

    [Header("Components")]
    [SerializeField] private MeshRenderer meshRenderer = null;

    [Header("Configuration")]
    [SerializeField] private uint cameraId = 1u;

    private new Transform transform;


    /// <summary>
    /// Gets or sets the id of the associated PhaseSpace camera.
    /// </summary>
    public uint CameraId { get => this.cameraId; set => this.cameraId = value; }



    /// <summary>
    /// Will be called when the component gets initialized.
    /// </summary>
    private void Awake() {
        //this.AssertComponentOrAttachedInChildren(ref this.meshRenderer);

        this.meshRenderer.enabled = false;
        this.transform = this.GetComponent<Transform>();

    }

    /// <summary>
    /// Will be called when the component has been initialized.
    /// </summary>
    private void Start() {
       // App.Instance.PhaseSpaceManager?.OnCameraFrame?.AddListener(this.OnCameraFrame);
    }



    /// <summary>
    /// Will be called when new camera data has been received.
    /// </summary>
    /// <param name="e">The arguments of the event.</param>
    private void OnCameraFrame(PhaseSpaceCameraEventArgs e) {
        if ( e.Camera.id == this.cameraId ) {
            this.meshRenderer.enabled = e.Camera.Condition > PhaseSpace.Unity.TrackingCondition.Invalid;
            this.transform.localPosition = e.Camera.position;
            this.transform.localRotation = e.Camera.rotation;
        }
    }

}