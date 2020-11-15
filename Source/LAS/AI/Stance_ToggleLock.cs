using LAS.Utility;
using Verse;

namespace LAS
{
    public class Stance_ToggleLock : Stance_Busy
    {
        private LockComp lockComp;
        public Stance_ToggleLock(int ticks, LocalTargetInfo focusTarg, Verb verb) : base(ticks, focusTarg, verb)
        {
            if (!(focusTarg.Thing is ThingWithComps twc) || !twc.IsLock(out lockComp))
                return;
            if (lockComp.InstalledThing != null)
                this.focusTarg = lockComp.InstalledThing;
        }
        public void Interrupt()
        {
            lockComp.InterruptToggle();
            base.Expire();
        }
        public override void StanceTick()
        {
            if (--ticksLeft <= 0)
            {
                if (lockComp.Toggling)
                    ticksLeft = 1;
                else
                    Expire();
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref lockComp, "lockComp");
        }
    }
}
