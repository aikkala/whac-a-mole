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
        private float _reward, _previousPoints, _initialPoints;
        private bool _isFinished;
        private Transform _marker;
        private string _game;
        private string _level;
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
                _game = UitBUtils.GetKeywordArgument("game");
                _level = UitBUtils.GetKeywordArgument("level");
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
                _game = "difficulty";
                _level = "level2";
                _fixedSeed = 0;
                _logging = false;
            }
            Debug.Log("RLEnv set to game " + _game + " and level " + _level);

            // Enable logging if necessary
            logger.enabled = _logging;
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
                    _reward += (float)(Math.Exp(-10*dist)-1) / 10;
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
            sequenceManager.playParameters.game = _game;
            sequenceManager.playParameters.level = _level;
            sequenceManager.playParameters.Initialise(true, _fixedSeed);
            
            // Visit Ready state, as some important stuff will be set (on exit)
            sequenceManager.stateMachine.GotoState(GameState.Ready);

            // Start playing
            sequenceManager.stateMachine.GotoState(GameState.Play);
            
            // Reset points
            _previousPoints = sequenceManager.Points;
        }
    }
}