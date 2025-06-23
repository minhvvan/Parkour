using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class InAirParkourState: BaseParkourState
    {
        private float _rotationVelocity;
        private float _maxJumpSpeed;

        public InAirParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
            _maxJumpSpeed = Mathf.Sqrt(2.0f * _playerBlackBoard.playerMovementData.gravity * _playerBlackBoard.playerMovementData.jumpForce);
        }

        public override void OnUpdate(ParkourContext context)
        {
            if (context.isGrounded)
            {
                _parkourCharacterController.jumpTimeout -= Time.deltaTime;

                if (_parkourCharacterController.verticalVelocity <= 0 && _parkourCharacterController.jumpTimeout <= 0)
                {
                    _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.OnGround);
                }
            }
            else
            {
                var velocity = _parkourCharacterController.horizontalVelocity;
                Vector3 targetDirection = _parkourCharacterController.transform.forward;
                
                if (_playerBlackBoard.parkourInputController.moveInput != Vector2.zero)
                {
                    // 점프중에는 입력 방향으로 이동하도록
                    Vector3 inputDirection = new Vector3(_playerBlackBoard.parkourInputController.moveInput.x, 0.0f, _playerBlackBoard.parkourInputController.moveInput.y).normalized;

                    var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _playerBlackBoard.mainCamera.transform.eulerAngles.y;
                    velocity += inputDirection.magnitude * Time.deltaTime;
                    
                    targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
                    float rotation = Mathf.SmoothDampAngle(_parkourCharacterController.transform.eulerAngles.y, targetRotation, ref _rotationVelocity, _playerBlackBoard.playerMovementData.rotationSmoothTime);

                    _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
                
                _playerBlackBoard.characterController.Move(targetDirection.normalized * velocity);
                _parkourCharacterController.jumpTimeout = _playerBlackBoard.playerMovementData.jumpCoolDown;
            }
        
            if (!_playerBlackBoard.animator.IsUnityNull())
            {
                var verticalAnimVale = (1 - _parkourCharacterController.verticalVelocity / _maxJumpSpeed) * 0.5f;
            
                _playerBlackBoard.animator.SetFloat(AnimationHash.VerticalAnimID, verticalAnimVale);
                _playerBlackBoard.animator.SetFloat(AnimationHash.JumpTimeoutAnimID, _parkourCharacterController.jumpTimeout);
            }
        }

        public override void OnExit()
        {
        }

        public override void HandleAnimation()
        {
            
        }
    }
}