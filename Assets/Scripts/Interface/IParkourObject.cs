using Player;
using Player.Parkour;

namespace Interface
{
    public interface IParkourObject
    {
        public ParkourObjectType parkourObjectType { get; }
        public bool CanPerform();
        public void Perform(ParkourCharacterController controller);
    }
}