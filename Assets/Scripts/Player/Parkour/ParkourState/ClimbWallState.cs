using Player.Parkour;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.ParkourState
{
    public class ClimbWallState: BaseParkourState
    {
        private float _rotationVelocity;
        private float _maxJumpSpeed;

        public ClimbWallState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard) : base(parkourCharacterController, playerBlackBoard)
        {
        }

        public override void OnEnter()
        {
            _playerBlackBoard.animator.SetTrigger(AnimationHash.ClimbWall);
            _parkourCharacterController.applyGravity = false;
        }

        public override void OnUpdate(ParkourContext context)
        {
            var currentState = _playerBlackBoard.animator.GetCurrentAnimatorStateInfo(0);
            
            if (currentState.IsName("ClimbWall") && currentState.normalizedTime >= .9)
            {
                _parkourCharacterController.ChangeParkourState(Parkour.ParkourState.OnGround);
            }
        }

        public override void OnExit()
        {
            _parkourCharacterController.applyGravity = true;
        }

        public override void HandleAnimation()
        {
            var deltaPosition = _playerBlackBoard.animator.deltaPosition;
            var deltaRotation = _playerBlackBoard.animator.deltaRotation;
            
            _playerBlackBoard.characterController.Move(deltaPosition);
            _playerBlackBoard.transform.rotation *= deltaRotation;
        }
    }
}