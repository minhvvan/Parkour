using Player.Parkour;
using UnityEngine;

namespace Player.ParkourState
{
    public class SlopeSlippingParkourState : BaseParkourState
    {
        private Vector3 _gravitySlope;
        private float _targetRotation;
        private float _rotationVelocity;
        
        public SlopeSlippingParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
            _playerBlackBoard.animator.SetTrigger(AnimationHash.SlopeSlipAnimID);
            _gravitySlope = Vector3.ProjectOnPlane(Physics.gravity, _playerBlackBoard.footTracker.groundNormal);
        }

        public override void OnUpdate(ParkourContext context)
        {
            _targetRotation = Mathf.Atan2(_gravitySlope.x, _gravitySlope.z) * Mathf.Rad2Deg;
            _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);

            if (_playerBlackBoard.footTracker.groundAngle < _playerBlackBoard.playerMovementData.slopeLimit)
            {
                _playerBlackBoard.animator.SetTrigger(AnimationHash.SlopeSlipEndAnimID);
                _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.OnGround);
            }
        }

        public override void OnExit()
        {
            _playerBlackBoard.animator.SetFloat(AnimationHash.SpeedAnimID, 0f);
            _gravitySlope = Vector3.zero;
        }

        public override void HandleAnimation()
        {
            Vector3 deltaPosition = _playerBlackBoard.animator.deltaPosition;
            Quaternion deltaRotation = _playerBlackBoard.animator.deltaRotation;
            
            _playerBlackBoard.characterController.Move(deltaPosition);
            
            if (deltaRotation != Quaternion.identity)
            {
                _parkourCharacterController.transform.rotation *= deltaRotation;
            }
        }
    }
}