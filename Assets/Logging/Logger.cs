using UnityEngine;


/// <summary>
/// Represents the base class for all logging interfaces.
/// </summary>
public abstract class Logger : MonoBehaviour {

    /// <summary>
    /// Will be called when the component gets initialized.
    /// </summary>
    protected virtual void Awake() {
        this.enabled = false;
    }

}