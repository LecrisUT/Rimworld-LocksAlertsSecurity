using LAS.Utility;
using RimWorld;
using Verse;

namespace LAS
{
    public class Stance_ToggleDoor : Stance_Busy
    {
        private DoorLockComp doorLockComp;
        public Stance_ToggleDoor(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
            if (!(focusTarg.Thing is Building_Door door) || !door.HasDoorLock(out doorLockComp))
                return;
        }
        public void Interrupt()
        {
            doorLockComp.InterruptToggle();
            base.Expire();
        }
        public override void StanceTick()
        {
            if (--ticksLeft <= 0)
            {
                if (doorLockComp.Toggling)
                    ticksLeft = 1;
                else
                    Expire();
            }
        }
        protected override void Expire()
        {
            if (stanceTracker.curStance == this)
            {
                if (doorLockComp.assignedState.IsLocked())
                    stanceTracker.SetStance(new Stance_Mobile());
                else
                {
                    var door = doorLockComp.Door;
                    var pawn = Pawn;
                    if (!door.Open)
                        door.StartManualOpenBy(pawn);
                    pawn.stances.SetStance(new Stance_Cooldown(door.TicksTillFullyOpened, door, null) { neverAimWeapon = true });
                    door.CheckFriendlyTouched(pawn);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref doorLockComp, "doorLockComp");
        }
    }
}
