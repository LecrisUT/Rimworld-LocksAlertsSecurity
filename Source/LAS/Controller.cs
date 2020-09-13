using HugsLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace LAS
{
    internal class Controller : ModBase
    {
        public override void DefsLoaded()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(t=>t.thingClass == typeof(Building_Door)))
            {
                if (!def.HasComp(typeof(DoorLockComp)))
                {
                    if (def.comps.NullOrEmpty())
                        def.comps = new List<CompProperties>();
                    def.comps.Add(new CompProperties(typeof(DoorLockComp)));
                }
                if (!def.HasModExtension<LockExtension>())
                {
                    if (def.modExtensions.NullOrEmpty())
                        def.modExtensions = new List<DefModExtension>();
                    def.modExtensions.Add(new LockExtension());
                }
            }
        }
    }
}
