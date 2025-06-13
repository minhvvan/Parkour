using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ParkourCharacterController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController parkourCharacterController;
    [SerializeField] private ParkourInputController parkourInputController;
    [SerializeField] private CinemachineBrain mainCamera;
    [SerializeField] private GameObject cinemachineCameraTarget;
    
    [SerializeField] private PlayerMovementDataSo playerMovementData;

    private bool _grounded;
    private float _verticalVelocity;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity = 0.0f;
    private float _targetSpeed = 0.0f;
    private float _speed = 0.0f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    
    private const float _threshold = 0.01f;

    private void Start()
    {
        _grounded = true;
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
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void GroundCheck()
    {
        var checkPosition = transform.position;
        checkPosition.y -= .1f;
        
        _grounded = Physics.CheckSphere(checkPosition, 0.2f);
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            if (_verticalVelocity < 0)
            {
                _verticalVelocity = -2f;
            }
            
            if (parkourInputController.jump)
            {
                _verticalVelocity = Mathf.Sqrt(2.0f * playerMovementData.gravity * playerMovementData.jumpForce);
            }
        }
        else
        {
            parkourInputController.jump = false;
        }
        
        _verticalVelocity -= playerMovementData.gravity * Time.deltaTime;
    }

    private void Move()
    {
        _targetSpeed = playerMovementData.moveSpeed;

        Vector3 inputDirection = new Vector3(parkourInputController.moveInput.x, 0.0f, parkourInputController.moveInput.y).normalized;

        if (parkourInputController.moveInput != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, playerMovementData.rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        else
        {
            _targetSpeed = 0f;
        }
        
        float currentHorizontalSpeed = new Vector3(parkourCharacterController.velocity.x, 0.0f, parkourCharacterController.velocity.z).magnitude;
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < _targetSpeed - speedOffset || currentHorizontalSpeed > _targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, _targetSpeed * parkourInputController.moveInput.magnitude, Time.deltaTime * playerMovementData.speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = _targetSpeed;
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        parkourCharacterController.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }
}
