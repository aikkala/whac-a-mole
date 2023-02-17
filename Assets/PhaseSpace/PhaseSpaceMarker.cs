using UnityEngine;


/// <summary>
/// Represents a component to visualize a PhaseSpace marker.
/// </summary>
public class PhaseSpaceMarker : MonoBehaviour {

    [Header("Components")]
    [SerializeField] private MeshRenderer meshRenderer = null;

    [Header("Configuration")]
    [SerializeField] private bool display = true;
    [SerializeField] private uint markerId = 1u;


    /// <summary>
    /// Gets or sets the id of the associated PhaseSpace marker.
    /// </summary>
    public uint MarkerId { get => this.markerId; set => this.markerId = value; }

    /// <summary>
    /// Gets the position of the marker in PhaseSpace coordinates.
    /// </summary>
    public Vector3 PhaseSpacePosition { get; private set; }

    /// <summary>
    /// Gets velocity of the marker in PhaseSpace coordinates.
    /// </summary>
    public Vector3 PhaseSpaceVelocity { get; private set; }

    /// <summary>
    /// Gets the position of the marker in world coordinates.
    /// </summary>
    public Vector3 WorldPosition => this.transform.position;

    /// <summary>
    /// Gets the velocity of the marker in world coordinates.
    /// </summary>
    //public Vector3 WorldVelocity => App.Instance.SpaceManager.TransformPhaseSpaceToWorldVector(this.PhaseSpaceVelocity);



    /// <summary>
    /// Will be called when the component gets initialized.
    /// </summary>
    protected virtual void Awake() {
        //this.AssertComponentOrAttachedInChildren(ref this.meshRenderer);

        this.meshRenderer.enabled = false;
    }

    /// <summary>
    /// Will be called when the component has been initialized.
    /// </summary>
    protected virtual void Start() {
        //Game.Instance.PhaseSpaceManager?.OnMarkerFrame?.AddListener(this.OnMarkerFrame);
    }



    /// <summary>
    /// Will be called when new marker data has been received.
    /// </summary>
    /// <param name="e">The arguments of the event.</param>
    protected virtual void OnMarkerFrame(PhaseSpaceMarkerEventArgs e) {
        if ( e.Marker.id == this.markerId ) {
            this.meshRenderer.enabled = this.display && e.Marker.Condition > PhaseSpace.Unity.TrackingCondition.Invalid;
            this.PhaseSpacePosition = e.Marker.position;
            this.PhaseSpaceVelocity = e.Marker.velocity;
            this.transform.localPosition = e.Marker.position;
        }
    }

}