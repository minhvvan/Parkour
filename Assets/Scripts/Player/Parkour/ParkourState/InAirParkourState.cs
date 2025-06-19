using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class InAirParkourState: BaseParkourState
    {
        private static readonly int VerticalAnimID = Animator.StringToHash("Vertical");
        private static readonly int JumpTimeoutAnimID = Animator.StringToHash("JumpTimeout");

        private Vector3 _jumpMomentum;
        
        public InAirParkourState(ParkourCharacterController parkourCharacterController) : base(
            parkourCharacterController)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnUpdate(ParkourContext context)
        {
            var maxJumpSpeed = Mathf.Sqrt(2.0f * context.movementSettingData.gravity * context.movementSettingData.jumpForce);

            if (context.isGrounded)
            {
                if (_parkourCharacterController.jumpTimeout <= 0)
                {
                    AnimatorStateInfo currentState = context.animator.GetCurrentAnimatorStateInfo(0);

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
                Vector3 inputDirection = new Vector3(context.moveInput.x, 0.0f, context.moveInput.y).normalized;

                var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + context.cameraTransform.eulerAngles.y;
                var velocity = _parkourCharacterController.horizontalVelocity + inputDirection.magnitude * Time.deltaTime;

                Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
                context.characterController.Move(targetDirection.normalized * velocity);
            
                _parkourCharacterController.jumpTimeout = context.movementSettingData.jumpCoolDown;
            }
        
            if (!context.animator.IsUnityNull())
            {
                var verticalAnimVale = (1 - _parkourCharacterController.verticalVelocity / maxJumpSpeed) * 0.5f;
            
                context.animator.SetFloat(VerticalAnimID, verticalAnimVale);
                context.animator.SetFloat(JumpTimeoutAnimID, _parkourCharacterController.jumpTimeout);
            }
        }

        public override void OnExit()
        {
        }
    }
}