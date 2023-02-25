// MemeRestriction.cs created by Iron Wolf for Pawnmorph on 07/22/2021 5:34 PM
// last updated 07/22/2021  5:34 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// restriction for a def, requiring the pawn to have at least one meme from the given list
	/// </summary>
	/// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
	public class MemeRestriction : RestrictionExtension
	{
		/// <summary>
		/// list of memes the pawn's ideo must have one of to pass the list 
		/// </summary>
		public List<MemeDef> requiredMemes = new List<MemeDef>();


		/// <summary>
		/// list of memes the pawn's ideo must not have'
		/// </summary>
		public List<MemeDef> restrictedMemes = new List<MemeDef>();

		[Unsaved, NotNull] private readonly Dictionary<Ideo, bool> _cache = new Dictionary<Ideo, bool>();

		/// <summary>
		///     checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			var ideo = pawn.Ideo;
			if (ideo == null)
				return requiredMemes.Count == 0;

			if (!_cache.TryGetValue(ideo, out bool val))
			{
				val = CheckIdeo(ideo);

				_cache[ideo] = val;
			}

			return val;
		}

		private bool CheckIdeo(Ideo ideo)
		{
			bool val = requiredMemes.Count == 0;
			foreach (MemeDef memeDef in requiredMemes)
			{
				if (ideo.HasMeme(memeDef))
				{
					val = true;
					break;
				}
			}

			foreach (MemeDef restrictedMeme in restrictedMemes)
			{
				if (ideo.HasMeme(restrictedMeme))
				{
					val = false;
				}
			}

			return val;
		}
	}
}