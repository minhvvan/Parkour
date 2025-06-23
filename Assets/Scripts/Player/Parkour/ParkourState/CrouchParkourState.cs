using Player.Parkour;
using UnityEngine;

namespace Player.ParkourState
{
    public class CrouchParkourState : BaseParkourState
    {
        private float _targetRotation;
        private float _rotationVelocity;
        
        public CrouchParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
            _playerBlackBoard.animator.SetBool(AnimationHash.Crouch, true);
        }

        public override void OnUpdate(ParkourContext context)
        {
            var moveInput = _playerBlackBoard.parkourInputController.moveInput;
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg * _playerBlackBoard.mainCamera.transform.eulerAngles.y;
                _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
            }
        }

        public override void OnExit()
        {
            _playerBlackBoard.animator.SetBool(AnimationHash.Crouch, false);
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