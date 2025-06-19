using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class InAirParkourState: BaseParkourState
    {


        private float _rotationVelocity;

        public InAirParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnUpdate(ParkourContext context)
        {
            var maxJumpSpeed = Mathf.Sqrt(2.0f * _playerBlackBoard.playerMovementData.gravity * _playerBlackBoard.playerMovementData.jumpForce);

            if (context.isGrounded)
            {
                if (_parkourCharacterController.jumpTimeout <= 0)
                {
                    AnimatorStateInfo currentState = _playerBlackBoard.animator.GetCurrentAnimatorStateInfo(0);

                    if (!currentState.IsName("Landing") && !currentState.IsName("Jump"))
                    {
                        _parkourCharacterController.verticalVelocity = maxJumpSpeed;
                    }
                }
                
                _parkourCharacterController.jumpTimeout -= Time.deltaTime;

                if (_parkourCharacterController.verticalVelocity <= 0 && _parkourCharacterController.jumpTimeout <= 0)
                {
                    _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.OnGround);
                }
            }
            else
            {
                // 점프중에는 입력 방향으로 이동하도록
                Vector3 inputDirection = new Vector3(_playerBlackBoard.parkourInputController.moveInput.x, 0.0f, _playerBlackBoard.parkourInputController.moveInput.y).normalized;

                var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _playerBlackBoard.mainCamera.transform.eulerAngles.y;
                var velocity = _parkourCharacterController.horizontalVelocity + inputDirection.magnitude * Time.deltaTime;

                Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
                float rotation = Mathf.SmoothDampAngle(_parkourCharacterController.transform.eulerAngles.y, targetRotation, ref _rotationVelocity, _playerBlackBoard.playerMovementData.rotationSmoothTime);

                _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                _playerBlackBoard.characterController.Move(targetDirection.normalized * velocity);
                
                _parkourCharacterController.jumpTimeout = _playerBlackBoard.playerMovementData.jumpCoolDown;
            }
        
            if (!_playerBlackBoard.animator.IsUnityNull())
            {
                var verticalAnimVale = (1 - _parkourCharacterController.verticalVelocity / maxJumpSpeed) * 0.5f;
            
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