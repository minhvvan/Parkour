using System.Collections.Generic;
using Player;
using Player.Parkour;
using Player.ParkourState;
using Unity.VisualScripting;
using UnityEngine;

public class ParkourCharacterController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerBlackBoard blackBoard;
    [SerializeField] private GameObject cinemachineCameraTarget;
    
    // Parkour State
    private ParkourState _currentParkourState;
    private Dictionary<ParkourState, BaseParkourState> _parkourStates = new Dictionary<ParkourState, BaseParkourState>();
    
    // slope
    private bool _grounded;
    
    public bool applyGravity = true;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    public float jumpTimeout = 0.0f;
    public float verticalVelocity;
    public float horizontalVelocity;

    
    private const float _cameraRotateThreshold = 0.01f;

    private void Start()
    {
        blackBoard.Initialize();
        
        _grounded = true;
        applyGravity = true;
        
        // 상태 생성
        _parkourStates[ParkourState.OnGround] = new OnGroundParkourState(this, blackBoard);
        _parkourStates[ParkourState.InAir] = new InAirParkourState(this, blackBoard);
        _parkourStates[ParkourState.SlopeSlip] = new SlopeSlippingParkourState(this, blackBoard);
        
        _currentParkourState = ParkourState.OnGround;
    }

    private void Update()
    {
        ApplyGravity();
        GroundCheck();
        
        if (blackBoard.parkourInputController.jump && jumpTimeout <= 0)
        {
            blackBoard.parkourInputController.jump = false;
            AnimatorStateInfo currentState = blackBoard.animator.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsName("Landing") && !currentState.IsName("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(2.0f * blackBoard.playerMovementData.gravity * blackBoard.playerMovementData.jumpForce);
            }
        }

        var context = new ParkourContext()
        {
            isGrounded = _grounded,
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
        if (blackBoard.parkourInputController.lookInput.sqrMagnitude >= _cameraRotateThreshold)
        {
            _cinemachineTargetYaw += blackBoard.parkourInputController.lookInput.x;
            _cinemachineTargetPitch += blackBoard.parkourInputController.lookInput.y;
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
        var checkPosition = transform.position + blackBoard.characterController.center;
        checkPosition.y -=  (blackBoard.characterController.height/2 + blackBoard.playerMovementData.groundedOffset);
    
        var currentGrounded = Physics.CheckSphere(checkPosition, blackBoard.playerMovementData.groundedRadius, blackBoard.playerMovementData.groundLayer, QueryTriggerInteraction.Ignore);
        // 발이 떨어지는 순간
        if(_grounded && !currentGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, float.MaxValue,
                    blackBoard.playerMovementData.groundLayer, QueryTriggerInteraction.Ignore))
            {
                _grounded = hit.distance < blackBoard.playerMovementData.fallThreshold;
                if (!_grounded) ChangeParkourState(ParkourState.InAir);
            }
        }
        else
        {
            _grounded = currentGrounded;
        }
    
        if (!blackBoard.animator.IsUnityNull())
        {
            blackBoard.animator.SetBool(AnimationHash.GroundedAnimID, _grounded);
        }
    }

    private void ApplyGravity()
    {
        if (!applyGravity) return;
        
        // 중력 적용
        verticalVelocity -= blackBoard.playerMovementData.gravity * Time.deltaTime;
        
        if (_grounded && verticalVelocity <= 0)
        {
            verticalVelocity = -Mathf.Sqrt(2.0f * blackBoard.playerMovementData.gravity * blackBoard.playerMovementData.jumpForce);
        }
        blackBoard.characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        _parkourStates[_currentParkourState].HandleAnimation();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = _grounded ? transparentGreen : transparentRed;

        var checkPosition = transform.position + blackBoard.characterController.center;
        checkPosition.y -=  (blackBoard.characterController.height/2 + blackBoard.playerMovementData.groundedOffset);
        Gizmos.DrawSphere(checkPosition, blackBoard.playerMovementData.groundedRadius);
    }
}
