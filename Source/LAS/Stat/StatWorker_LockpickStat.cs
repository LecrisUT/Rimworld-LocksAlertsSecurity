using LAS.Utility;
using RimWorld;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LAS
{
    public class StatWorker_LockpickStat : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
            => req.Thing is Pawn || (req.Def is ThingDef tDef && (tDef.HasDoorLock(out _) || tDef.IsLock(out _)));
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is ThingWithComps twc)
            {
                if (twc.HasDoorLock(out var doorLock, false))
                    return doorLock.LockComps.Select(t => base.GetValueUnfinalized(StatRequest.For(t.parent), applyPostProcess))
                        .MaxByWithFallback(t => Mathf.Abs(t), 0f);
                return base.GetValueUnfinalized(req, applyPostProcess);
            }
            if (req.Def is ThingDef tDef)
            {
                if (tDef.HasDoorLock(out var doorLock))
                    return doorLock.defaultLocks.Select(t => base.GetValueUnfinalized(StatRequest.For(t, GenStuff.DefaultStuffFor(t)), applyPostProcess))
                        .MaxByWithFallback(t => Mathf.Abs(t), 0f);
                return base.GetValueUnfinalized(req, applyPostProcess);
            }
            return 0f;
        }
        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            var builder = new StringBuilder(base.GetExplanationUnfinalized(req, numberSense));
            if (req.Thing is ThingWithComps twc)
            {
                if (twc.HasDoorLock(out var doorLock, false))
                {
                    var bestLock = doorLock.LockComps.MaxByWithFallback(t => Mathf.Abs(base.GetValueUnfinalized(StatRequest.For(t.parent))))?.parent;
                    if (doorLock.LockComps.EnumerableNullOrEmpty())
                        builder.AppendLine("No locks installed");
                    else
                    {
                        builder.AppendLine("Installed locks:");
                        foreach (var lockComp in doorLock.LockComps)
                        {
                            builder.Append("  ");
                            var tLock = lockComp.parent;
                            if (tLock == bestLock)
                                builder.Append("Best: ");
                            var val = base.GetValueUnfinalized(StatRequest.For(tLock));
                            if (stat == StatDefOf.LockpickingSpeed)
                                val = 1 / val;
                            else if (stat == StatDefOf.LockpickingSuccess)
                                val = -val;
                            builder.AppendLine($"{tLock.LabelNoCount} : {val}");
                        }
                    }
                    return builder.ToString();
                }
                return builder.ToString();
            }
            if (req.Def is ThingDef tDef && tDef.HasDoorLock(out var doorLockProp))
            {
                builder.AppendLine("Pre-Installed locks:");
                var bestLock = doorLockProp.defaultLocks.MaxByWithFallback(t => Mathf.Abs(base.GetValueUnfinalized(StatRequest.For(t, GenStuff.DefaultStuffFor(t)))));
                foreach (var tLock in doorLockProp.defaultLocks)
                {
                    builder.Append("  ");
                    var stuff = GenStuff.DefaultStuffFor(tLock);
                    if (tLock == bestLock)
                        builder.Append("Best: ");
                    var val = base.GetValueUnfinalized(StatRequest.For(tDef, stuff));
                    if (stat == StatDefOf.LockpickingSpeed)
                        val = 1 / val;
                    else if (stat == StatDefOf.LockpickingSuccess)
                        val = -val;
                    builder.AppendLine($"{GenLabel.ThingLabel(tLock, stuff)} : {val}");
                }
            }
            return builder.ToString();
        }
        public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
        {
            base.FinalizeValue(req, ref val, applyPostProcess);
            if (req.Def is ThingDef tDef && (tDef.HasDoorLock(out _) || tDef.IsLock(out _)))
            {
                if (stat == StatDefOf.LockpickingSpeed)
                    val = 1 / val;
                else if (stat == StatDefOf.LockpickingSuccess)
                    val = -val;
            }
        }
    }
}
