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
        private LockState state;
        public LockState State => state;
        public bool TrySetState(LockState state)
        {
            if (this.state == LockState.Broken && state != LockState.Broken)
                return false;
            this.state = state;
            return true;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref automatic, "automatic");
        }
    }
}
