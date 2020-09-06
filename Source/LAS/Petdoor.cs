using Verse;

namespace LAS
{
    public class Petdoor : IExposable
    {
        public enum Size
        {
            Small,
            Medium,
            Large,
        }
        public Size size;
        public Lock Lock;
        public void ExposeData()
        {
            Scribe_Values.Look(ref size, "size");
            Scribe_Deep.Look(ref Lock, "Lock");
        }
    }
}
