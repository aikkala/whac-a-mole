using System;
using UnityEngine;


namespace UserInTheBox
{
    public class RLEnv_Whacamole : RLEnv
    {
        // This class implements the RL environment for the Whacamole game.

        public SequenceManager sequenceManager;
        public Func<float, float> _distRewardFunc;
        public bool _useRewardSplines=true;
        private float _previousPoints, _initialPoints, _previousContacts, _initialContacts, _elapsedTimeScaled;
        private Transform _marker;
        private string _condition;
        private int _fixedSeed;

        public override void InitialiseReward()
        {
            _initialPoints = sequenceManager.Points;
            _previousPoints = _initialPoints;
            _initialContacts = sequenceManager.Contacts;
            _previousContacts = _initialContacts;
            _marker = simulatedUser.rightHandController.Find("Hammer/marker").transform;

            if (_useRewardSplines)
            {
                var _xmax = 0.8f;  //*100  #initial/maximum distance that can be typically reached; used to scale entire reward term relative to other rewards
                var _ymax = -1.0f;  //minimum negative reward; used to scale entire reward term relative to other rewards
                // var _xeps = _playParameters.targetSize[1];
                var _xeps = 0.025f;  //*100  #"sufficient" distance to fulfill the pointing task (typically, this corresponds to the target radius); often, if dist<=_xeps (for the first time), an additional bonus reward is given
                var _yeps = -0.0f;  //-0.05  #reward given at target boundary (WARNING: needs to be non-positive!); should be chosen between 10%*_ymax and _ymin=0
                var _xref = 0.3f;  //*100  #"expected" distance; used to scale gradients of this reward term appropriately
                // we set _positive_only to true, as otherwise the maximum distance reward of zero is also given whenever a target disappears but the next ones are not available yet (happens quite often in the easy condition with only one target at once)
                var _positive_only = true;  //whether to ensure that all reward terms are non-negative (WARNING: non-negative values only guaranteed if initial distance _xmax is the maximum reachable distance!)

                var curve = new HermiteCurve();
                curve.Initialise(maxDist:_xmax, minReward:_ymax, epsDist:_xeps, epsReward:_yeps, refDist:_xref, positiveOnly:_positive_only);
                _distRewardFunc = curve.Evaluate;
            }
            else
            {
                _distRewardFunc = dist => (float)(Math.Exp(-10*dist)-1) / 10;
            }
            
        }

        public override void InitialiseGame()
        {
            
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

        }

        public override void UpdateIsFinished()
        {
            // Update finished
            _isFinished = sequenceManager.stateMachine.currentState.Equals(GameState.Ready);
        }

        protected override void CalculateReward()
        {
            // Get hit points
            int points = sequenceManager.Points;
            _reward = (points - _previousPoints)*10;
            _previousPoints = points;

            // Get points for unsuccesful contacts as well
            int contacts = sequenceManager.Contacts;
            _reward += (contacts - _previousContacts)*2;
            _previousContacts = contacts;
            
            // Also calculate distance component
            foreach (var target in sequenceManager.targetArea.GetComponentsInChildren<Target>())
            {
                if (target.stateMachine.currentState == TargetState.Alive)
                {
                    var dist = Vector3.Distance(target.transform.position, _marker.position);
                    // _reward += (float)(Math.Exp(-10*dist)-1) / 10;
                    _reward += _distRewardFunc(dist);

                    // Console.WriteLine("dist: " + dist);
                    // Console.WriteLine("reward: " + _distRewardFunc(dist));
                }
            }
        }

        public override float GetTimeFeature()
        {
            return sequenceManager.GetTimeFeature();
        }


        public override void Reset()
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