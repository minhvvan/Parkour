using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Player.Parkour;
using Player.ParkourState;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ParkourCharacterController : MonoBehaviour
{
    private static readonly int VerticalAnimID = Animator.StringToHash("Vertical");
    private static readonly int GroundedAnimID = Animator.StringToHash("Grounded");
    private static readonly int JumpTimeoutAnimID = Animator.StringToHash("JumpTimeout");

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ParkourInputController parkourInputController;
    [SerializeField] private CinemachineBrain mainCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private Animator animator;
    [SerializeField] private FootTracker footTracker;
    [SerializeField] private PlayerMovementDataSo playerMovementData;

    // Parkour State
    private ParkourState _currentParkourState;
    private Dictionary<ParkourState, BaseParkourState> _parkourStates = new Dictionary<ParkourState, BaseParkourState>();
    
    public bool applyGravity = true;
    private bool _grounded;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    public float jumpTimeout = 0.0f;
    public float verticalVelocity;
    public float horizontalVelocity;

    private Vector3 _slopeMovement;
    
    private const float _cameraRotateThreshold = 0.01f;

    private void Start()
    {
        _grounded = true;
        applyGravity = true;
        
        // 상태 생성
        _parkourStates[ParkourState.OnGround] = new OnGroundParkourState(this);
        _parkourStates[ParkourState.InAir] = new InAirParkourState(this);
        _parkourStates[ParkourState.SlopeSlip] = new SlopeSlippingParkourState(this);
        
        _currentParkourState = ParkourState.OnGround;
    }

    private void Update()
    {
        ApplyGravity();
        GroundCheck();
        
        if (parkourInputController.jump && jumpTimeout <= 0)
        {
            parkourInputController.jump = false;
            ChangeParkourState(ParkourState.InAir);
        }

        var context = new ParkourContext()
        {
            moveInput = parkourInputController.moveInput,
            isSprinting = parkourInputController.sprint,
            isGrounded = _grounded,
            cameraTransform = mainCamera.transform,
            animator = animator,
            characterController = characterController,
            movementSettingData = playerMovementData,
            inputController = parkourInputController,
        };
        
        _parkourStates[_currentParkourState].OnUpdate(context);
    }

    public void ChangeParkourState(ParkourState newState)
    {
        if (_parkourStates[_currentParkourState] != null)
        {
            _parkourStates[_currentParkourState].OnExit();
        }

        _currentParkourState = newState;
        _parkourStates[_currentParkourState].OnEnter();
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (parkourInputController.lookInput.sqrMagnitude >= _cameraRotateThreshold)
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
        if (_currentParkourState == ParkourState.InAir)
        {
            _grounded = false;
            
            // 왼발
            {
                var checkPosition = footTracker.leftFootPosition;
                checkPosition.y -= playerMovementData.groundedOffset;
                _grounded |= Physics.CheckSphere(checkPosition, playerMovementData.groundedRadius, playerMovementData.groundLayer, QueryTriggerInteraction.Ignore);
            }
            
            // 오른발
            {
                var checkPosition = footTracker.rightFootPosition;
                checkPosition.y -= playerMovementData.groundedOffset;
                _grounded |= Physics.CheckSphere(checkPosition, playerMovementData.groundedRadius, playerMovementData.groundLayer, QueryTriggerInteraction.Ignore);
            }
        }
        else
        {
            var checkPosition = transform.position;
            checkPosition.y -= playerMovementData.groundedOffset;
        
            var currentGrounded = Physics.CheckSphere(checkPosition, playerMovementData.groundedRadius, playerMovementData.groundLayer, QueryTriggerInteraction.Ignore);
            // 발이 떨어지는 순간
            if(_grounded && !currentGrounded)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out var hit, float.MaxValue,
                        playerMovementData.groundLayer, QueryTriggerInteraction.Ignore))
                {
                    _grounded = hit.distance < playerMovementData.fallThreshold;
                }
            }
            else
            {
                _grounded = currentGrounded;
            }
        }
        
        if (!animator.IsUnityNull())
        {
            animator.SetBool(GroundedAnimID, _grounded);
        }
    }

    private void ApplyGravity()
    {
        if (!applyGravity) return;
        
        // 중력 적용
        verticalVelocity -= playerMovementData.gravity * Time.deltaTime;
        characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if (_currentParkourState == ParkourState.InAir) return;
        
        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;
            
        // 이동량 감지 및 처리
        if (deltaPosition != Vector3.zero)
        {
            var speedMult = parkourInputController.sprint ? playerMovementData.sprintSpeed : playerMovementData.moveSpeed;
            
            if (footTracker.isOnSlope)
            {
                // 올라가지 못하는 경사 처리                
                if (footTracker.isUp && footTracker.groundAngle > playerMovementData.slopeLimit)
                {
                    _slopeMovement = Vector3.ProjectOnPlane(Physics.gravity, footTracker.groundNormal).normalized * deltaPosition.magnitude * speedMult;
                    Vector3.Normalize(_slopeMovement);

                    transform.rotation = Quaternion.Euler(_slopeMovement);
                }
                else
                {
                    _slopeMovement = Vector3.ProjectOnPlane(deltaPosition, footTracker.groundNormal).normalized * deltaPosition.magnitude * speedMult;
                }
                
                characterController.Move(_slopeMovement);
                horizontalVelocity = (new Vector3(_slopeMovement.x, 0, _slopeMovement.z)).magnitude;
            }
            else
            {
                characterController.Move(deltaPosition * speedMult);
                horizontalVelocity = (deltaPosition * speedMult).magnitude;
            }
        }
        else
        {
            horizontalVelocity = 0f;
        }
      
        if (deltaRotation != Quaternion.identity)
        {
            transform.rotation *= deltaRotation;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = _grounded ? transparentGreen : transparentRed;

        if (_currentParkourState == ParkourState.InAir)
        {
            var left = footTracker.leftFootPosition;
            var right = footTracker.rightFootPosition;

            left.y -= playerMovementData.groundedOffset;
            right.y -= playerMovementData.groundedOffset;
            
            Gizmos.DrawSphere(left, playerMovementData.groundedRadius);
            Gizmos.DrawSphere(right, playerMovementData.groundedRadius);
        }
        else
        {
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - playerMovementData.groundedOffset, transform.position.z), playerMovementData.groundedRadius);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + _slopeMovement * 10);
    }
}
