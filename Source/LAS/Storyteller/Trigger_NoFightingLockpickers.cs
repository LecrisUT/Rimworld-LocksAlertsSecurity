using LAS.Utility;
using Verse;
using Verse.AI.Group;

namespace LAS.Storyteller
{
    public class Trigger_NoFightingLockpickers : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				for (int i = 0; i < lord.ownedPawns.Count; i++)
				{
					Pawn p = lord.ownedPawns[i];
					if (!p.Downed && !p.InMentalState && p.IsGoodLockpicker())
						return false;
				}
				return true;
			}
			return false;
		}
	}
}
