using LAS.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace LAS.Storyteller
{
    public class LordToil_AssaultColonyLockpicker : LordToil
    {
		private static readonly FloatRange EscortRadiusRanged = new FloatRange(15f, 19f);
		private static readonly FloatRange EscortRadiusMelee = new FloatRange(23f, 26f);
		private LordToilData_AssaultColonySappers Data => (LordToilData_AssaultColonySappers)data;
		public override bool AllowSatisfyLongNeeds => false;
		public override bool ForceHighStoryDanger => true;
		public LordToil_AssaultColonyLockpicker()
		{
			data = new LordToilData_AssaultColonySappers();
		}
		public override void Init()
		{
			base.Init();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
		}
		public override void UpdateAllDuties()
		{
			if (!Data.sapperDest.IsValid && lord.ownedPawns.Any())
				Data.sapperDest = GenAI.RandomRaidDest(lord.ownedPawns[0].Position, Map);
			List<Pawn> list = null;
			if (Data.sapperDest.IsValid)
			{
				list = new List<Pawn>();
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn pawn = lord.ownedPawns[i];
					if (pawn.IsGoodLockpicker())
						list.Add(pawn);
				}
			}
			for (int k = 0; k < lord.ownedPawns.Count; k++)
			{
				Pawn pawn3 = lord.ownedPawns[k];
				if (list?.Contains(pawn3) ?? false)
					pawn3.mindState.duty = new PawnDuty(DutyDefOf.Lockpicker, Data.sapperDest);
				else if (!list.NullOrEmpty())
				{
					float radius = (pawn3.equipment == null || pawn3.equipment.Primary == null || !pawn3.equipment.Primary.def.IsRangedWeapon) ? EscortRadiusMelee.RandomInRange : EscortRadiusRanged.RandomInRange;
					pawn3.mindState.duty = new PawnDuty(RimWorld.DutyDefOf.Escort, list.RandomElement(), radius);
				}
				else
					pawn3.mindState.duty = new PawnDuty(RimWorld.DutyDefOf.AssaultColony);
			}
		}
		public override void Notify_ReachedDutyLocation(Pawn pawn)
		{
			Data.sapperDest = IntVec3.Invalid;
			UpdateAllDuties();
		}
	}
}
