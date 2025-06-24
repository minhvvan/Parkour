using Interface;
using Unity.VisualScripting;
using UnityEngine;

namespace Player.Parkour.ParkourObjects
{
    public class ParkourWall: MonoBehaviour, IParkourObject
    {
        [SerializeField] private ParkourObjectType _parkourObjectType = ParkourObjectType.Wall;
        public ParkourObjectType parkourObjectType => _parkourObjectType;

        public bool CanPerform()
        {
            return true;
        }

        public void Perform(ParkourCharacterController controller)
        {
            var characterPos = controller.transform.position;
            
            var dotResult = Vector3.Dot(characterPos - transform.position, transform.forward);
            if (dotResult < 0)
            {
                controller.transform.rotation = transform.rotation;
            }
            else
            {
                controller.transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            
            controller.ChangeParkourState(ParkourState.ClimbWall);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }
}