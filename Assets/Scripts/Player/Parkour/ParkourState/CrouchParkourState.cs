using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class CrouchParkourState : BaseParkourState
    {
        private float _targetRotation;
        private float _rotationVelocity;
        private float _animSpeed;
        private float _targetSpeed;
        
        public CrouchParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
            _playerBlackBoard.animator.SetBool(AnimationHash.CrouchAnimID, true);
        }

        public override void OnUpdate(ParkourContext context)
        {
            if (!_playerBlackBoard.parkourInputController.crouch)
            {
                _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.OnGround);
                return;
            }
            
            var moveInput = _playerBlackBoard.parkourInputController.moveInput;
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + _playerBlackBoard.mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(_parkourCharacterController.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _playerBlackBoard.playerMovementData.rotationSmoothTime);

                _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                _targetSpeed = 1f;
            }
            else
            {
                _targetSpeed = 0f;
            }
            
            if (!_playerBlackBoard.animator.IsUnityNull())
            {
                float alpha = Time.deltaTime * _playerBlackBoard.playerMovementData.speedChangeRate;
                alpha *= _playerBlackBoard.parkourInputController.moveInput != Vector2.zero ? 1 : _playerBlackBoard.playerMovementData.decelerateRate;
                
                _animSpeed = Mathf.Lerp(_animSpeed, _targetSpeed, alpha);
                _playerBlackBoard.animator.SetFloat(AnimationHash.SpeedAnimID, _animSpeed);
            }
        }

        public override void OnExit()
        {
            _playerBlackBoard.animator.SetBool(AnimationHash.CrouchAnimID, false);
        }

        public override void HandleAnimation()
        {
            Vector3 deltaPosition = _playerBlackBoard.animator.deltaPosition;

            if (!_playerBlackBoard.animator.IsUnityNull())
            {
                var currentState = _playerBlackBoard.animator.GetCurrentAnimatorStateInfo(0);
                if (currentState.IsName("SprintCrouch"))
                {
                    deltaPosition *= Mathf.Max(1, _playerBlackBoard.animator.GetFloat(AnimationHash.SpeedAnimID));
                }
            }
            
            _playerBlackBoard.characterController.Move(deltaPosition);
        }
    }
}