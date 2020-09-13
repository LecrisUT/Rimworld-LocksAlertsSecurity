using System.Collections.Generic;
using Verse;

namespace LAS
{
    public class MultipleThingDesignation : Designation, IExposable
    {
        public MultipleThingDesignation() { }
        public MultipleThingDesignation(LocalTargetInfo targetInfo, DesignationDef def, List<Thing> things) : base(targetInfo, def)
        {
            this.things = things;
        }
        public List<Thing> things;
        public int maxThings = int.MaxValue;
        public virtual new void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref things, "things");
            Scribe_Values.Look(ref maxThings, "maxThings", int.MaxValue);
        }
    }
}
