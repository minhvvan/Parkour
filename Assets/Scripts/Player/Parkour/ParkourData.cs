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
        public static readonly int JumpStartAnimID = Animator.StringToHash("JumpStart");
        public static readonly int CrouchAnimID = Animator.StringToHash("Crouch");
        public static readonly int ClimbWall = Animator.StringToHash("ClimbWall");
    }
    
    public enum ParkourState
    {
        None = 0,
        OnGround,
        InAir,
        SlopeSlip,
        Crouch,
        ClimbWall,
        Max
    }
    
    public enum ParkourObjectType
    {
        None = 0,
        Wall,
        Max
    }
    
    public struct ParkourContext
    {
        public bool isGrounded;
    }
}