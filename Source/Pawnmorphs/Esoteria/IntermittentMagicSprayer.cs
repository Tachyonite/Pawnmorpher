using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;

namespace Pawnmorph{

public class IntermittentMagicSprayer
{
	//Links
	private Thing parent;

	//Working vars
	int ticksUntilSpray = MinTicksBetweenSprays;
	int sprayTicksLeft = 0;
	public Action startSprayCallback = null;
	public Action endSprayCallback = null;

	//Constants
	private const int MinTicksBetweenSprays = 2;
	private const int MaxTicksBetweenSprays = 5;
	private const int MinSprayDuration = 1;
	private const int MaxSprayDuration = 3;
	private const float SprayThickness = 0.6f;

        public static ThingDef Mote_MagicPuff = ThingDef.Named("Mote_MagicPuff");

        public IntermittentMagicSprayer(Thing parent)
	{
		this.parent = parent;
	}

        private static MoteThrown NewBaseMagicPuff()
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(Mote_MagicPuff, null);
            moteThrown.Scale = 1.5f;
            moteThrown.rotationRate = (float)Rand.RangeInclusive(-240, 240);
            return moteThrown;
        }

        public static void ThrowMagicPuffUp(Vector3 loc, Map map)
        {
            if (!loc.ToIntVec3().ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = IntermittentMagicSprayer.NewBaseMagicPuff();
            moteThrown.exactPosition = loc;
            moteThrown.exactPosition += new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
            moteThrown.SetVelocity((float)Rand.Range(-10, 10), Rand.Range(1.2f, 1.5f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public static void ThrowMagicPuffDown(Vector3 loc, Map map)
        {
            if (!loc.ToIntVec3().ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = IntermittentMagicSprayer.NewBaseMagicPuff();
            moteThrown.exactPosition = loc + new Vector3(Rand.Range(-0.02f, 0.02f), 0f, Rand.Range(-0.02f, 0.02f));
            moteThrown.SetVelocity((float)Rand.Range(0,359), Rand.Range(0.2f, 0.5f));
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public void SteamSprayerTick()
	{
		if( sprayTicksLeft > 0 )
		{
			sprayTicksLeft--;

			//Do spray effect
			if( Rand.Value < SprayThickness )
                    IntermittentMagicSprayer.ThrowMagicPuffUp( parent.TrueCenter(), parent.Map);	

			//Push some heat
			if( Find.TickManager.TicksGame % 20 == 0 )
			{
				//GenTemperature.PushHeat(parent, 40 );
			}

			//Done spraying
			if( sprayTicksLeft <= 0 )
			{
				if( endSprayCallback != null )
					endSprayCallback();

				ticksUntilSpray = Rand.RangeInclusive( MinTicksBetweenSprays, MaxTicksBetweenSprays );
			}
		}
		else
		{
			ticksUntilSpray--;

			if( ticksUntilSpray <= 0 )
			{
				//Start spray
				if( startSprayCallback != null )
					startSprayCallback();
				
				sprayTicksLeft = Rand.RangeInclusive( MinSprayDuration, MaxSprayDuration );
			}
		}
	}
}}

