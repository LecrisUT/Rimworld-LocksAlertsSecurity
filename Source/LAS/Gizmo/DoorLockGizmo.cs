using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LAS
{
    public class DoorLockGizmo : Command_Toggle
    {
        private Building_Door door;
        private DoorLockComp doorLock;
        public DoorLockGizmo(Building_Door door, DoorLockComp doorLock)
        {
            this.door = door;
            this.doorLock = doorLock;
            isActive = delegate { return this.doorLock.LockState == Lock.LockState.Unlocked; };
            toggleAction = delegate
            {
                var des = this.door.Map.designationManager;
                var togdes = des.DesignationOn(this.door, DesignationDefOf.ToggleDoorLock);
                if (togdes != null)
                    des.RemoveDesignation(togdes);
                else
                {
                    des.AddDesignation(new Designation(this.door, DesignationDefOf.ToggleDoorLock));
                    if (isActive())
                        doorLock.AssignLockState(Lock.LockState.Locked);
                    else
                        doorLock.AssignLockState(Lock.LockState.Unlocked);
                }
            };
        }
    }
}
