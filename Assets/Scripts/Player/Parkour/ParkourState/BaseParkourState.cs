using Player.Parkour;
using UnityEngine;

namespace Player.ParkourState
{
    public abstract class BaseParkourState
    {
        public ParkourCharacterController _parkourCharacterController;

        public BaseParkourState(ParkourCharacterController parkourCharacterController)
        {
            _parkourCharacterController = parkourCharacterController;
        }

        public abstract void OnEnter();

        public abstract void OnUpdate(ParkourContext context);

        public abstract void OnExit();
    }
}