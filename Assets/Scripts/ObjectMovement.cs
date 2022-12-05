using UnityEngine;

public class ObjectMovement : MonoBehaviour {

  private Vector3 _localNormal = new Vector3(0, -1, 0);
  private Vector3 _velocity;
  private Vector3 _oldPos;
  
  public float Speed => _velocity.magnitude;
  public Vector3 MovementDirection => _velocity.normalized;
  public Vector3 NormalDirection => transform.TransformDirection(_localNormal);
  public Vector3 Velocity => _velocity;

  void Start() {
    _oldPos = transform.position;
    _velocity = new Vector3();
  }

  void FixedUpdate() {
    var currVelocity = (transform.position - _oldPos) / Time.deltaTime;
    _oldPos = transform.position;
    _velocity = Vector3.Lerp(_velocity, currVelocity, 0.5f);
  }
}
