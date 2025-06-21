using UnityEngine;

namespace Player.Parkour
{
    public struct AnimationHash
    {
        public static readonly int SpeedAnimID = Animator.StringToHash("Speed");
        public static readonly int VerticalAnimID = Animator.StringToHash("Vertical");
        public static readonly int JumpTimeoutAnimID = Animator.StringToHash("JumpTimeout");
        public static readonly int GroundedAnimID = Animator.StringToHash("Grounded");
        public static readonly int SlopeSlipAnimID = Animator.StringToHash("SlopeSlip");
        public static readonly int SlopeSlipEndAnimID = Animator.StringToHash("SlopeSlipEnd");
        public static readonly int JumpStart = Animator.StringToHash("JumpStart");
    }
    
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
        public bool isGrounded;
    }
}