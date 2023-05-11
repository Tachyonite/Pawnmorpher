using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace Pawnmorph.Utilities
{
	internal static class HashGiverUtils
	{
		private static Action<Def, Type, HashSet<ushort>> _GiveHashMethod;
		private static AccessTools.FieldRef<Dictionary<Type, HashSet<ushort>>> _takenHashesDictionary;

		static HashGiverUtils()
		{
			_GiveHashMethod = AccessTools.MethodDelegate<Action<Def, Type, HashSet<ushort>>>(AccessTools.Method(typeof(ShortHashGiver), "GiveShortHash", new Type[] { typeof(Def), typeof(Type), typeof(HashSet<ushort>) }));
			_takenHashesDictionary = AccessTools.StaticFieldRefAccess<Dictionary<Type, HashSet<ushort>>>(AccessTools.Field(typeof(ShortHashGiver), "takenHashesPerDeftype"));
		}

		internal static void GiveShortHash<T>(T def) where T : Def
		{
			Dictionary<Type, HashSet<ushort>> dictionary = _takenHashesDictionary();

			if (dictionary.ContainsKey(typeof(T)) == false)
				dictionary[typeof(T)] = new HashSet<ushort>();

			HashSet<ushort> takenHashes = dictionary[typeof(T)];
			_GiveHashMethod(def, null, takenHashes);
		}
	}
}
