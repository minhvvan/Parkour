using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class OnGroundParkourState : BaseParkourState
    {
        private static readonly int SpeedAnimID = Animator.StringToHash("Speed");

        private float _targetSpeed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _animSpeed;
        
        
        public OnGroundParkourState(ParkourCharacterController parkourCharacterController) : base(parkourCharacterController)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnUpdate(ParkourContext context)
        {
            Vector3 inputDirection = new Vector3(context.moveInput.x, 0.0f, context.moveInput.y).normalized;

            if (context.moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + context.cameraTransform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(_parkourCharacterController.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, context.movementSettingData.rotationSmoothTime);

                _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                _targetSpeed = context.isSprinting ? 2 : 1;
            }
            else
            {
                _targetSpeed = 0f;
            }

            if (!context.animator.IsUnityNull())
            {
                float alpha = Time.deltaTime * context.movementSettingData.speedChangeRate;
                alpha *= context.moveInput != Vector2.zero ? 1 : context.movementSettingData.decelerateRate;
                
                _animSpeed = Mathf.Lerp(_animSpeed, _targetSpeed, alpha);
                context.animator.SetFloat(SpeedAnimID, _animSpeed);
            }
        }

        public override void OnExit()
        {
            _targetSpeed = 0f;
            _targetRotation = 0f;
            _rotationVelocity = 0f;
            _animSpeed = 0f;
        }
    }
}