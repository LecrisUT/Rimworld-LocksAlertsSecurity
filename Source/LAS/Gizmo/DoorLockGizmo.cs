using LAS.Utility;
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
            // icon = ContentFinder<Texture2D>.Get(DesignationDefOf.ToggleDoorLock.texturePath);
            this.door = door;
            this.doorLock = doorLock;
            isActive = delegate { return this.doorLock.State.IsLocked(); };
            toggleAction = delegate
            {
                var des = this.door.Map.designationManager;
                var togdes = des.DesignationOn(this.door, DesignationDefOf.ToggleDoorLock);
                if (togdes != null)
                    des.RemoveDesignation(togdes);
                else
                {
                    des.AddDesignation(new Designation(this.door, DesignationDefOf.ToggleDoorLock));
                    doorLock.assignedState = isActive() ? LockComp.LockState.Default : LockComp.LockState.Locked;
                }
            };
        }
        public override Color IconDrawColor => isActive() ? Color.red : Color.green;
        protected override void DrawIcon(Rect rect, Material buttonMat = null)
        {
            icon = isActive() ? ContentFinder<Texture2D>.Get("Icons/Locked") : ContentFinder<Texture2D>.Get("Icons/Unlocked");
            base.DrawIcon(rect, buttonMat);
        }
    }
}
