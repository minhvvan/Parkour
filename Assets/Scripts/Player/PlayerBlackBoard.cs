using Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerBlackBoard: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private ParkourInputController _parkourInputController;
        [SerializeField] private CinemachineBrain _mainCamera;
        [SerializeField] private Animator _animator;
        [SerializeField] private FootTracker _footTracker;
        [SerializeField] private PlayerMovementDataSo _playerMovementData;
        [SerializeField] private ParkourSystem _parkourSystem;
        
        public CharacterController characterController { get; private set; }
        public ParkourInputController parkourInputController  { get; private set; }
        public CinemachineBrain mainCamera  { get; private set; }
        public Animator animator  { get; private set; }
        public FootTracker footTracker  { get; private set; }
        public PlayerMovementDataSo playerMovementData  { get; private set; }
        public ParkourSystem parkourSystem  { get; private set; }

        public void Initialize()
        {
            characterController = _characterController;
            parkourInputController = _parkourInputController;
            mainCamera = _mainCamera;
            animator = _animator;
            footTracker = _footTracker;
            playerMovementData = _playerMovementData;
            parkourSystem = _parkourSystem;
        }
    }
}