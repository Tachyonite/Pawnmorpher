// CompCacher.cs created by Iron Wolf for Pawnmorph on 08/22/2020 1:10 PM
// last updated 08/22/2020  1:10 PM

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph.Utilities
{
	internal static class GenCompCacher
	{
		[NotNull] private static readonly Type[] _scratchArr;

		[NotNull] private static readonly Type[] _pawnTypeScratchArr;

		static GenCompCacher()
		{
			CompTypes = new List<Type>();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				try
				{
					LoadTypesFrom(assembly, CompTypes);
				}
				catch (Exception e)
				{
					Log.Error($"Pawnmorpher: {nameof(GenCompCacher)} caught exception {e.GetType().Name} while loading types from {assembly.FullName}!\n{e}");
				}

			_scratchArr = new Type[1];
			_pawnTypeScratchArr = new[] { typeof(Pawn) };
		}

		private static Type CompCacherGenType { get; } = typeof(CompCacher<>);

		[NotNull] private static List<Type> CompTypes { get; }


		public static void ClearAllCompCaches()
		{
			foreach (Type compType in CompTypes)
			{
				MethodInfo methodInfo = GetClearMethod(compType);
				methodInfo.Invoke(null, Array.Empty<object>());
			}
		}

		private static MethodInfo GetClearMethod(Type compType)
		{
			_scratchArr[0] = compType;
			//var flgs = BindingFlags.Static | BindingFlags.Public; 
			return CompCacherGenType.MakeGenericType(_scratchArr)
									.GetMethod(nameof(CompCacher<ThingComp>.ClearCache), Type.EmptyTypes);
		}

		private static void LoadTypesFrom([NotNull] Assembly assembly, [NotNull] List<Type> compTypes)
		{
			foreach (Type type in assembly.GetTypes())
				if (typeof(ThingComp).IsAssignableFrom(type) && !type.IsAbstract)
					compTypes.Add(type);
		}
	}

	/// <summary>
	///     static class for caching comps for pawns
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public static class CompCacher<T> where T : ThingComp
	{
		[NotNull] private static readonly Dictionary<Pawn, T> _compCache = new Dictionary<Pawn, T>();

		/// <summary>
		///     Clears the cache for this particular pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static void ClearCache([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			_compCache.Remove(pawn);
		}

		/// <summary>
		///     Clears the cache for all pawns
		/// </summary>
		public static void ClearCache()

		{
			_compCache.Clear();
		}


		/// <summary>
		///     Gets the comp cached.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		[CanBeNull]
		public static T GetCompCached([NotNull] Pawn pawn)
		{
			if (_compCache.TryGetValue(pawn, out T comp)) return comp;
			var c = pawn.GetComp<T>();
			if (c != null) //don't cache misses 
				_compCache[pawn] = c;
			return c;
		}
	}

	/// <summary>
	///     world comp to refresh the comp cacher on load
	/// </summary>
	/// <seealso cref="RimWorld.Planet.WorldComponent" />
	public class CompCacherWComp : WorldComponent
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CompCacherWComp" /> class.
		/// </summary>
		/// <param name="world">The world.</param>
		public CompCacherWComp(World world) : base(world)
		{
		}


		/// <summary>
		///     Finalizes the initialize.
		/// </summary>
		public override void FinalizeInit()
		{
			base.FinalizeInit();
			try
			{
				GenCompCacher.ClearAllCompCaches(); //clear any pawns from a previous world 
			}
			catch (Exception)
			{
				Log.Error($"Pawnmorpher is unable to clear all comp caches on world initialization!");
			}
		}
	}
}