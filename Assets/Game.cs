using PhaseSpace.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents the main component of the Gamelication.
/// </summary>
public class Game : MonoBehaviour {


    [Header("Components")]
    [SerializeField] private OWLClient phaseSpaceClient = null;
    [SerializeField] private PhaseSpaceManager phaseSpaceManager = null;
    [SerializeField] private SequenceManager sequenceManager = null;

    private readonly List<Logger> loggers = new List<Logger>();

    private static Game instance = null;


    /// <summary>
    /// Gets the list of loggers registered in the Game.
    /// </summary>
    public IReadOnlyList<Logger> Loggers => this.loggers.AsReadOnly();
    /// <summary>
    /// Gets the PhaseSpace client.
    /// </summary>
    public OWLClient PhaseSpaceClient => this.phaseSpaceClient;
    /// <summary>
    /// Gets the PhaseSpace manager.
    /// </summary>
    public PhaseSpaceManager PhaseSpaceManager => this.phaseSpaceManager;
    /// <summary>
    /// Gets the Sequence manager.
    /// </summary>
    public SequenceManager SequenceManager => this.sequenceManager;
    

    /// <summary>
    /// Gets the global instance of the <see cref="Game"/> component.
    /// </summary>
    public static Game Instance {
        get {
            if ( !instance ) {
                instance = FindObjectOfType<Game>();
                if ( !instance ) {
                    instance = null;
                }
            }

            return instance;
        }
    }



    /// <summary>
    /// Will be called when the component gets initialized.
    /// </summary>
    private void Awake() {
        if ( instance && instance != this ) {
            Debug.LogWarning($"There are multiple experiment managers in the scene. {this.name} will be destroyed.");
            Destroy(this);
            return;
        }

        instance = this;

        //this.AssertComponentOrInScene(ref this.phaseSpaceClient);
        //this.AssertComponentOrInScene(ref this.phaseSpaceManager);

        this.loggers.AddRange(FindObjectsOfType<Logger>());

        this.sequenceManager.onGameStarted.AddListener(this.StartLogging);
        //this.sequenceManager.onExperimentCancelled.AddListener(this.StopLogging);
        this.sequenceManager.onGameFinished.AddListener(this.StopLogging);
    }

    /// <summary>
    /// Starts the data logging.
    /// </summary>
    public void StartLogging() {
        foreach ( var logger in this.loggers ) {
            logger.enabled = true;
        }
    }

    /// <summary>
    /// Stops the data logging.
    /// </summary>
    public void StopLogging() {
        foreach ( var logger in this.loggers ) {
            logger.enabled = false;
        }
    }

}