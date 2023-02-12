using RimWorld;
using Verse;

namespace EtherGun
{
	class Building_TrapTransforming : Building_TrapExplosive
	{
		protected override void SpringSub(Pawn p)
		{
			Log.Error("Building_TrapTransforming.SpringSub()");
			GetComp<CompEtherExplosive>().StartWick(null);
		}
	}
}
