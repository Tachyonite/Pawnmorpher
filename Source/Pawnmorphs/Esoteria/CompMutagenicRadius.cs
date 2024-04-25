using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// comp for mutating things within a radius 
	/// </summary>
	public class CompMutagenicRadius : ThingComp
	//TODO see if code can be reused (centrifuge, liquidslury, mutagenicstone, PMPlantUtilities...)
	{
		private const float LEAFLESS_PLANT_KILL_CHANCE = 0.09f;
		private const float MUTATE_IN_RADIUS_CHANCE = 0.50f;
		//determines how quickly mutagenic effects falls off with distance 
		private const float DISTANCE_POW = 0.8004695f;


		private int plantHarmAge;
		private int ticksToPlantHarm;

		CompProperties_MutagenicRadius PropsPlantHarmRadius
		{
			get
			{
				return (CompProperties_MutagenicRadius)props;
			}
		}
		/// <summary>
		/// call to save/load data 
		/// </summary>
		public override void PostExposeData()
		{
			Scribe_Values.Look(ref plantHarmAge, "plantHarmAge", 0, false);
			Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 0, false);
			Scribe_Values.Look(ref _radius, nameof(Radius), 0);
		}

		[NotNull]
		private readonly List<Pawn> _pawnsCache = new List<Pawn>();

		private float _radius;




		//set this constant to increase/decrease the radius growth rate for debugging 
		private const float EVAL_MULTIPLIER = 1;


		/// <summary>
		/// Gets the radius.
		/// </summary>
		/// <value>
		/// The radius.
		/// </value>
		public float Radius
		{
			get => _radius;
			private set => _radius = value;
		}

		/// <summary>
		/// called every tick after it's parent updates 
		/// </summary>
		public override void CompTick()
		{
			if (parent.IsHashIntervalTick(60))
			{
				if (!parent.Spawned)
				{
					return;
				}
				plantHarmAge += 60;
				ticksToPlantHarm--;
				if (ticksToPlantHarm <= 0)
				{
					float x = plantHarmAge / 60000f;
					Radius = PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x * EVAL_MULTIPLIER);
					float num2 = Mathf.PI * Radius * Radius;
					float num3 = num2 * PropsPlantHarmRadius.harmFrequencyPerArea;
					float num4 = 60f / num3;
					int num5;

					if (num4 >= 1f)
					{
						ticksToPlantHarm = GenMath.RoundRandom(num4);
						num5 = 1;
					}
					else
					{
						ticksToPlantHarm = 1;
						num5 = GenMath.RoundRandom(1f / num4);
					}

					for (int i = 0; i < num5; i++)
					{
						MutateInRadius(Radius, PropsPlantHarmRadius.hediff);
					}
				}
			}

			if (parent.IsHashIntervalTick(540))
			{
				_pawnsCache.Clear();
				float x = plantHarmAge / 60000f;
				float num = PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x * EVAL_MULTIPLIER) * Rand.Range(0.7f, 1f);
				num = Mathf.Min(num, GenRadial.MaxRadialPatternRadius - EPSILON);
				var pawns = GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, num, true)
									 .MakeSafe()
									 .OfType<Pawn>()
									 .Where(p => MutagenDefOf.defaultMutagen.CanInfect(p));
				_pawnsCache.AddRange(pawns);
			}


		}

		private const float EPSILON = 0.01f;

		private const float A = EPSILON * BASE_BUILDUP_RATE;
		private void MutateInRadius(float radius, HediffDef hediff)
		{
			IntVec3 c = parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
			if (!c.InBounds(parent.Map))
			{
				return;
			}

			foreach (Pawn pawn in _pawnsCache)
			{
				float distanceTo = pawn.Position.DistanceTo(parent.Position);
				if (distanceTo < radius) //make pawns closer to the mutagenic ship mutate faster 
				{    //also increase the effect as the radius increases 
					float rHat = distanceTo / radius;
					float baseMRate;
					if (rHat <= EPSILON) baseMRate = BASE_BUILDUP_RATE;
					else
					{
						baseMRate = A / (Mathf.Pow(rHat, DISTANCE_POW));
					}

					MutatePawn(parent.def, pawn, baseMRate);
				}
			}



			Plant plant = c.GetPlant(parent.Map);

			if (plant != null && !plant.def.IsMutantPlant()) //don't harm mutant plants 
			{
				if (!plant.LeaflessNow)
				{
					plant.MakeLeafless(Plant.LeaflessCause.Poison);
				}
				else if (Rand.Value < 0.3f) //30% chance
				{
					PMPlantUtilities.TryMutatePlant(plant);
				}
			}


		}

		private const float BASE_BUILDUP_RATE = 0.007984825f;

		private static void MutatePawn([CanBeNull] Def source, Pawn pawn, float baseBuildupRate)
		{
			if (pawn != null && MutagenDefOf.defaultMutagen.CanInfect(pawn))
			{
				if (Rand.Value < MUTATE_IN_RADIUS_CHANCE)
				{
					var num = baseBuildupRate;
					num *= pawn.GetMutagenicBuildupMultiplier();
					if (num != 0f)
					{
						float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x46EDC5D)); //should be ok
						num *= num2; //what's the magic number? 
						MutagenicBuildupUtilities.AdjustMutagenicBuildup(source, pawn, num);

					}
				}

			}
		}
	}
}
