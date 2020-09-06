using Verse;

namespace LAS
{
    public class Lock : IExposable
    {
        public enum LockState
        {
            Locked,
            Unlocked,
            Broken,
        }
        public bool automatic = false;
        public LockState state;
        public void ExposeData()
        {
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref automatic, "automatic");
        }
    }
}
