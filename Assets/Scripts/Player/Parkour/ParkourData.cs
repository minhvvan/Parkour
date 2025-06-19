using UnityEngine;

namespace Player.Parkour
{
    public enum ParkourState
    {
        None = 0,
        OnGround,
        InAir,
        SlopeSlip,
        Max
    }
    
    public struct ParkourContext
    {
        public Vector2 moveInput;
        public bool isSprinting;
        public bool isGrounded;
        public Transform cameraTransform;
        public Animator animator;
        public CharacterController characterController;
        public PlayerMovementDataSo movementSettingData;
        public ParkourInputController inputController;
    }
}