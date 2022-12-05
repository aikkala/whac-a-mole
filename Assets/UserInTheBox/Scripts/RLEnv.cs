using UnityEngine;


namespace UserInTheBox
{
    public class RLEnv : MonoBehaviour
    {
        // This class needs to be separately implemented for each different game, as we don't know the game
        // dynamics (how/where rewards are received from, which state game is in, etc.)

        public Transform headset;
        public SequenceManager sequenceManager;
        public SimulatedUser simulatedUser;
        private float _reward, _previousPoints, _initialPoints;
        private bool _isFinished;

        public void Start()
        {
            _initialPoints = sequenceManager.Points;
            _previousPoints = _initialPoints;
        }
        
        public void Update()
        {
            // Update reward
            int points = sequenceManager.Points;
            _reward = points - _previousPoints;
            _previousPoints = points;
            
            // Update finished
            _isFinished = sequenceManager.stateMachine.currentState.Equals(GameState.Done) || 
                          sequenceManager.stateMachine.currentState.Equals(GameState.Ready);
        }
        
        public float GetReward()
        {
            return _reward;
        }

        public bool IsFinished()
        {
            return _isFinished;
        }

        public void Reset()
        {
            // Update headset position (might not have been updated before reset is called the first time)
            // headset.transform.position = simulatedUser._server.GetSimulationState().headsetPosition;
            
            // Go to next state
            sequenceManager.stateMachine.GotoNextState();

            // Reset points
            _previousPoints = sequenceManager.Points;
        }
    }
}