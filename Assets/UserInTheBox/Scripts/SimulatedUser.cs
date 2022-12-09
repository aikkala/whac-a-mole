using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace UserInTheBox
{
    public class SimulatedUser : MonoBehaviour
    {
        public Transform leftHandController, rightHandController;
        public Camera mainCamera;
        public RLEnv env;
        private ZmqServer _server;
        private string _port;
        private Rect _rect;
        private RenderTexture _renderTexture;
        private Texture2D _tex;
        private bool _sendReply;

        public void Awake()
        {
            // Check if the this behaviour should be used
            enabled = UitBUtils.GetOptionalArgument("simulated");
            //enabled = true;

            if (enabled)
            {
                // Disable camera always; less rendering, less computations?
                mainCamera.enabled = false;
                // Disable the TrackedPoseDriver as well, otherwise XR Origin will always
                // try to reset position of camera to (0,0,0)?
                mainCamera.GetComponent<TrackedPoseDriver>().enabled = false;
            }
            else
            {
                // If SimulatedUser is not enabled, deactivate it and all its children
                gameObject.SetActive(false);
            }
        }

        public void Start()
        {
            // Initialise ZMQ server
            _port = UitBUtils.GetKeywordArgument("port");
            _server = new ZmqServer(_port, 60);
            
            // Wait for handshake from user-in-the-box simulated user
            var timeOptions = _server.WaitForHandshake();
            
            // Try to run the simulations as fast as possible
            Time.timeScale = timeOptions.timeScale; // Use an integer here!

            // Set policy query frequency
            Application.targetFrameRate = timeOptions.sampleFrequency*(int)Time.timeScale;
        
            // If fixedDeltaTime is defined in timeOptions use it, otherwise use timestep
            Time.fixedDeltaTime = timeOptions.fixedDeltaTime > 0 ? timeOptions.fixedDeltaTime : timeOptions.timestep;

            // Set maximum delta time
            Time.maximumDeltaTime = 1.0f / Application.targetFrameRate;
            
            Screen.SetResolution(1, 1, false);
            // TODO need to make sure width and height are set correctly, and then use Screen.height and Screen.width here instead of hardcoded values
            const int width = 120;
            const int height = 80;
            _rect = new Rect(0, 0, width, height);
            _renderTexture = new RenderTexture(width, height, 16);
            _tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        }
        
        public void Update()
        {
            _sendReply = false;
            SimulatedUserState previousState = _server.GetSimulationState();
            if (previousState != null && Time.fixedTime < previousState.nextTimestep)
            {
                return;
            }
            _sendReply = true;

            // Receive state from User-in-the-Box simulation
            var state = _server.ReceiveState();
        
            // Update anchors
            UpdateAnchors(state);

            // Check if we should quit the application, or reset environment
            if (state.quitApplication)
            {
                Application.Quit();
            }
            else if (state.reset)
            {
                env.Reset();
            }
        }
        
        private void UpdateAnchors(SimulatedUserState state)
        {
            mainCamera.transform.SetPositionAndRotation(state.headsetPosition, state.headsetRotation);
            leftHandController.SetPositionAndRotation(state.leftControllerPosition, state.leftControllerRotation);
            rightHandController.SetPositionAndRotation(state.rightControllerPosition, state.rightControllerRotation);
        }

        public void LateUpdate()
        {
            // Send response (observation) after frame has been rendered (and e.g. reward and termination condition 
            // have been updated in RLEnv)
            //StartCoroutine(SendObservationAtEndOfFrame());

            // If we didn't receive a state, don't send the observation
            if (!_sendReply)
            {
                return;
            }

            // Get agent camera and manually render scene into renderTexture
            mainCamera.targetTexture = _renderTexture;
            mainCamera.Render();

            // ReadPixels will read from the currently active render texture so make our offscreen 
            // render texture active and then read the pixels
            RenderTexture.active = _renderTexture;
            _tex.ReadPixels(_rect, 0, 0);

            // reset active camera texture and render texture
            mainCamera.targetTexture = null;
            RenderTexture.active = null;

            // Encode texture into PNG
            var image = _tex.EncodeToPNG();

            // Get reward
            var reward = env.GetReward();

            // Check if task is finished (terminated by either the app or the simulated user)
            var isFinished = env.IsFinished() || _server.GetSimulationState().isFinished;

            // Send observation to client
            _server.SendObservation(isFinished, reward, image);
        }

        private void OnDestroy()
        {
            _server?.Close();
        }

        private void OnApplicationQuit()
        {
            _server?.Close();
        }
    }
}