using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace UserInTheBox
{
    public class RLEnv : MonoBehaviour
    {
        // This class needs to be separately implemented for each different game, as we don't know the game
        // dynamics (how/where rewards are received from, which state game is in, etc.)

        public SequenceManager sequenceManager;
        public SimulatedUser simulatedUser;
        private float _reward, _previousPoints, _initialPoints;
        private bool _isFinished;
        private Transform _marker;

        public void Start()
        {
            _initialPoints = sequenceManager.Points;
            _previousPoints = _initialPoints;
            _marker = simulatedUser.rightHandController.Find("Hammer/marker").transform;
        }
        
        public void Update()
        {
            // Update reward
            CalculateReward();
            
            // Update finished
            _isFinished = sequenceManager.stateMachine.currentState.Equals(GameState.Ready);
        }

        private void CalculateReward()
        {
            // Get hit points
            int points = sequenceManager.Points;
            _reward = (points - _previousPoints)*10;
            _previousPoints = points;
            
            // Also calculate distance component
            foreach (var target in sequenceManager.targetArea.GetComponentsInChildren<Target>())
            {
                if (target.stateMachine.currentState == TargetState.Alive)
                {
                    var dist = Vector3.Distance(target.transform.position, _marker.position);
                    _reward += (float)(Math.Exp(-10*dist) - 1) / 10;
                }
            }
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
            // Set play level
            sequenceManager.playParameters.SetLevel("medium", true);
            
            // Visit Ready state, as some important stuff will be set (on exit)
            sequenceManager.stateMachine.GotoState(GameState.Ready);

            // Start playing
            sequenceManager.stateMachine.GotoState(GameState.Play);
            
            // Reset points
            _previousPoints = sequenceManager.Points;
        }
    }
}