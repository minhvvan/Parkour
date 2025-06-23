using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class OnGroundParkourState : BaseParkourState
    {
        private float _targetSpeed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _animSpeed;
        
        private bool _isClimbingSlope = false;
        private Vector3 _slopeVelocity = Vector3.zero; 
        private Vector3 _slopeMovement;
        
        public OnGroundParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
        }

        public override void OnUpdate(ParkourContext context)
        {
            if (_playerBlackBoard.parkourInputController.crouch)
            {
                _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.Crouch);
                return;
            }
            
            Vector3 inputDirection = new Vector3(_playerBlackBoard.parkourInputController.moveInput.x, 0.0f, _playerBlackBoard.parkourInputController.moveInput.y).normalized;

            if (_playerBlackBoard.parkourInputController.moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _playerBlackBoard.mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(_parkourCharacterController.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _playerBlackBoard.playerMovementData.rotationSmoothTime);

                _parkourCharacterController.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                _targetSpeed = _playerBlackBoard.parkourInputController.sprint ? 2 : 1;
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
            _targetSpeed = 0f;
            _targetRotation = 0f;
            _rotationVelocity = 0f;
            _animSpeed = 0f;
        }
        
        public override void HandleAnimation()
        {
            Vector3 deltaPosition = _playerBlackBoard.animator.deltaPosition;
            Quaternion deltaRotation = _playerBlackBoard.animator.deltaRotation;
                
            // 이동량 감지 및 처리
            if (deltaPosition != Vector3.zero)
            {
                var speedMult = _playerBlackBoard.parkourInputController.sprint ? _playerBlackBoard.playerMovementData.sprintSpeed : _playerBlackBoard.playerMovementData.moveSpeed;
                
                if (_playerBlackBoard.footTracker.isOnSlope)
                {                   
                    // 올라가지 못하는 경사 진입 시
                    if (_playerBlackBoard.footTracker.isUp && _playerBlackBoard.footTracker.groundAngle > _playerBlackBoard.playerMovementData.slopeLimit)
                    {
                        if (!_isClimbingSlope)
                        {
                            _slopeVelocity = Vector3.ProjectOnPlane(deltaPosition, _playerBlackBoard.footTracker.groundNormal).normalized * deltaPosition.magnitude * speedMult;
                            _isClimbingSlope = true;
                        }
                        
                        // 중력에 의한 저항 적용
                        var gravitySlope = Vector3.ProjectOnPlane(Physics.gravity, _playerBlackBoard.footTracker.groundNormal);
                        float resistanceStrength = Mathf.SmoothStep(0f, 1f, (_playerBlackBoard.footTracker.groundAngle - _playerBlackBoard.playerMovementData.slopeLimit) / (90f - _playerBlackBoard.playerMovementData.slopeLimit));
                        
                        _slopeVelocity += gravitySlope * resistanceStrength * _playerBlackBoard.playerMovementData.slopeResistanceMultiplier * Time.deltaTime;
                        
                        // 속도가 0 이하가 되면 미끄러짐 상태로 전환
                        float upwardSpeed = Vector3.Dot(_slopeVelocity, -gravitySlope.normalized);
                        if (upwardSpeed <= 0)
                        {
                            _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.SlopeSlip);
                            return;
                        }
                        
                        _playerBlackBoard.characterController.Move(_slopeVelocity);
                        _parkourCharacterController.horizontalVelocity = (new Vector3(_slopeVelocity.x, 0, _slopeVelocity.z)).magnitude;
                    }
                    else
                    {
                        // 일반 경사면
                        _isClimbingSlope = false;
                        _slopeMovement = Vector3.ProjectOnPlane(deltaPosition, _playerBlackBoard.footTracker.groundNormal).normalized * deltaPosition.magnitude * speedMult;
                        _playerBlackBoard.characterController.Move(_slopeMovement);
                        _parkourCharacterController.horizontalVelocity = (new Vector3(_slopeMovement.x, 0, _slopeMovement.z)).magnitude;
                    }
                }
                else
                {
                    // 평지
                    _isClimbingSlope = false;
                    _playerBlackBoard.characterController.Move(deltaPosition * speedMult);
                    _parkourCharacterController.horizontalVelocity = (deltaPosition * speedMult).magnitude;
                }
            }
            else
            {
                _parkourCharacterController.horizontalVelocity = 0f;
                _isClimbingSlope = false;
            }
          
            if (deltaRotation != Quaternion.identity)
            {
                _parkourCharacterController.transform.rotation *= deltaRotation;
            }
        }
    }
}