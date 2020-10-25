using RimWorld;
using Verse;

namespace LAS
{
    public interface IDoor
    {
        DoorLockComp LockComp { get; }
    }
    public class Door : Building_Door, IDoor
    {
        public DoorLockComp lockComp;
        public DoorLockComp LockComp
        {
            get
            {
                if (lockComp == null)
                    lockComp = GetComp<DoorLockComp>();
                return lockComp;
            }
        }
    }
}
