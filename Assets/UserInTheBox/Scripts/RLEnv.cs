using System;
using UnityEngine;


namespace UserInTheBox
{
    public class RLEnv : MonoBehaviour
    {
        // This class needs to be separately implemented for each different game, as we don't know the game
        // dynamics (how/where rewards are received from, which state game is in, etc.)

        public SequenceManager sequenceManager;
        public SimulatedUser simulatedUser;
        public Logger logger;
        private float _reward, _previousPoints, _initialPoints, _elapsedTimeScaled;
        private bool _isFinished;
        private Transform _marker;
        private string _condition;
        private int _fixedSeed;
        private bool _logging;

        public void Start()
        {
            // Don't run RLEnv if it is not needed
            if (!simulatedUser.enabled)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _initialPoints = sequenceManager.Points;
            _previousPoints = _initialPoints;
            _marker = simulatedUser.rightHandController.Find("Hammer/marker").transform;
            
            // Get game variant and level
            if (!simulatedUser.isDebug())
            {
                _condition = UitBUtils.GetKeywordArgument("condition");
                _logging = UitBUtils.GetOptionalArgument("logging");

                string fixedSeed = UitBUtils.GetOptionalKeywordArgument("fixedSeed", "0");
                // Try to parse given fixed seed string to int
                if (!Int32.TryParse(fixedSeed, out _fixedSeed))
                {
                    Debug.Log("Couldn't parse fixed seed from given value, using default 0");
                    _fixedSeed = 0;
                }

            }
            else
            {
                _condition = "medium";
                _fixedSeed = 0;
                _logging = false;
            }
            Debug.Log("RLEnv set to condition " + _condition);

            // Enable logging if necessary
            logger.enabled = _logging;
        }
        
        public void Update()
        {
            // Update reward
            CalculateReward();

            // Calculate how much time has elapsed in this round (scaled [-1, 1])
            CalculateTimeFeature();
            
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
                    _reward += (float)(Math.Exp(-10*dist)-1) / 10;
                }
            }
        }
        
        public float GetReward()
        {
            return _reward;
        }

        private void CalculateTimeFeature()
        {
            // Calculate how much time has elapsed in this round (scaled [-1, 1])
            _elapsedTimeScaled = (float)(((Time.time - sequenceManager._roundStart) / sequenceManager.playParameters.roundLength) - 0.5) * 2;
        }

        public float GetTimeFeature()
        {
            return _elapsedTimeScaled;
        }

        public bool IsFinished()
        {
            return _isFinished;
        }

        public void Reset()
        {
            // Set play level
            sequenceManager.playParameters.condition = _condition;
            sequenceManager.playParameters.Initialise(_fixedSeed);
            
            // Visit Ready state, as some important stuff will be set (on exit)
            sequenceManager.stateMachine.GotoState(GameState.Ready);

            // Start playing
            sequenceManager.stateMachine.GotoState(GameState.Play);
            
            // Reset points
            _previousPoints = sequenceManager.Points;
        }
    }
}