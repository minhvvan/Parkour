using Player.Parkour;
using UnityEngine;

namespace Player.ParkourState
{
    public abstract class BaseParkourState
    {
        public ParkourCharacterController _parkourCharacterController;
        public PlayerBlackBoard _playerBlackBoard;

        public BaseParkourState(ParkourCharacterController parkourCharacterController, PlayerBlackBoard playerBlackBoard)
        {
            _parkourCharacterController = parkourCharacterController;
            _playerBlackBoard = playerBlackBoard;
        }

        public abstract void OnEnter();

        public abstract void OnUpdate(ParkourContext context);

        public abstract void OnExit();

        public abstract void HandleAnimation();
    }
}