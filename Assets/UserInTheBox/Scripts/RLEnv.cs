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

        public void Start()
        {
            _initialPoints = sequenceManager.Points;
            _previousPoints = _initialPoints;
        }
        
        public void Update()
        {
            // Update reward
            CalculateReward();
            
            // Update finished
            _isFinished = sequenceManager.stateMachine.currentState.Equals(GameState.Done) || 
                          sequenceManager.stateMachine.currentState.Equals(GameState.Ready);
        }

        private void CalculateReward()
        {
            // Get hit points
            int points = sequenceManager.Points;
            _reward = points - _previousPoints;
            _previousPoints = points;

            // Also calculate distance component
            foreach (var target in sequenceManager.targetArea.GetComponentsInChildren<Target>())
            {
                if (target.stateMachine.currentState == TargetState.Alive)
                {
                    var dist = Vector3.Distance(target.transform.position,
                        simulatedUser.rightHandController.transform.position);
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
            // Find the play state (may need to go through multiple states if e.g. game has ended)
            while (sequenceManager.stateMachine.currentState != GameState.PlayRandom)
            {
                sequenceManager.stateMachine.GotoNextState();
            }

            // Reset points
            _previousPoints = sequenceManager.Points;
        }
    }
}