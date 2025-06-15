using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ParkourCharacterController : MonoBehaviour
{
    private static readonly int SpeedAnimID = Animator.StringToHash("Speed");
    private static readonly int VerticalAnimID = Animator.StringToHash("Vertical");
    private static readonly int GroundedAnimID = Animator.StringToHash("Grounded");
    private static readonly int JumpTimeoutAnimID = Animator.StringToHash("JumpTimeout");

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ParkourInputController parkourInputController;
    [SerializeField] private CinemachineBrain mainCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private Animator animator;
    
    [SerializeField] private PlayerMovementDataSo playerMovementData;

    private bool _grounded;
    private float _verticalVelocity;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity = 0.0f;
    private float _targetSpeed = 0.0f;
    private float _speed = 0.0f;
    private float _maxJumpSpeed = 0.0f;
    private float _jumpTimeout = 0.0f;
    private bool _isJumping = false;
    private Vector3 _jumpMomentum = Vector3.zero;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    
    private const float _threshold = 0.01f;

    private void Start()
    {
        _grounded = true;
        _maxJumpSpeed = Mathf.Sqrt(2.0f * playerMovementData.gravity * playerMovementData.jumpForce);
    }

    private void Update()
    {
        JumpAndGravity();
        GroundCheck();
        Move();
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (parkourInputController.lookInput.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += parkourInputController.lookInput.x;
            _cinemachineTargetPitch += parkourInputController.lookInput.y;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }
    
    private float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    private void GroundCheck()
    {
        var checkPosition = transform.position;
        checkPosition.y -= playerMovementData.groundedOffset;
        
        _grounded = Physics.CheckSphere(checkPosition, playerMovementData.groundedRadius, playerMovementData.groundLayer, QueryTriggerInteraction.Ignore);
        
        if (!animator.IsUnityNull())
        {
            animator.SetBool(GroundedAnimID, _grounded);
        }
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            // 땅에 있을 때 중력 적용
            if (_verticalVelocity <= 0)
            {
                _verticalVelocity = -2f;
                _isJumping = false;
            }
            
            if (parkourInputController.jump && _jumpTimeout <= 0)
            {
                parkourInputController.jump = false;
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

                if (!currentState.IsName("Landing") && !currentState.IsName("Jump"))
                {
                    _isJumping = true;
                    _verticalVelocity = _maxJumpSpeed;
                    _jumpMomentum = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
                }
            }

            _jumpTimeout -= Time.deltaTime;
        }
        else
        {
            _jumpTimeout = playerMovementData.jumpCoolDown;
        }
        
        if (!animator.IsUnityNull())
        {
            var verticalAnimVale = (1 - _verticalVelocity / _maxJumpSpeed) * 0.5f;
            if (_grounded && Mathf.Approximately(_verticalVelocity, -2))
            {
                verticalAnimVale = 1f;
            }
            
            animator.SetFloat(VerticalAnimID, verticalAnimVale);
            animator.SetFloat(JumpTimeoutAnimID, _jumpTimeout);
        }
        
        // 중력 적용
        _verticalVelocity -= playerMovementData.gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if (_isJumping) return;
        
        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;
            
        // 이동량 감지 및 처리
        if (deltaPosition != Vector3.zero)
        {
            var speedMult = parkourInputController.sprint ? playerMovementData.sprintSpeed : playerMovementData.moveSpeed;
            characterController.Move(deltaPosition * speedMult);
        }

        if (deltaRotation != Quaternion.identity)
        {
            transform.rotation *= deltaRotation;
        }
    }

    private void Move()
    {
        Vector3 inputDirection = new Vector3(parkourInputController.moveInput.x, 0.0f, parkourInputController.moveInput.y).normalized;

        if (parkourInputController.moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, playerMovementData.rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            _targetSpeed = parkourInputController.sprint ? 2 : 1;
        }
        else
        {
            _targetSpeed = 0f;
        }

        // 점프중에는 입력 방향으로 이동하도록
        if (_isJumping)
        {
            var velocity = _jumpMomentum.magnitude;
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            characterController.Move(targetDirection.normalized * (velocity * Time.deltaTime));
        }

        if (!animator.IsUnityNull())
        {
            _speed = Mathf.Lerp(_speed, _targetSpeed, Time.deltaTime * playerMovementData.speedChangeRate);
            animator.SetFloat(SpeedAnimID, _speed);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - playerMovementData.groundedOffset, transform.position.z), playerMovementData.groundedRadius);
    }
}
