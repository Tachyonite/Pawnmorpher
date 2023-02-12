using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	[StaticConstructorOnStartup]
	internal class ButtonTexturesPM
	{
		public static readonly Texture2D rotCW = ContentFinder<Texture2D>.Get("UI/Buttons/RotateCW");
		public static readonly Texture2D rotCCW = ContentFinder<Texture2D>.Get("UI/Buttons/RotateCCW");
		public static readonly Texture2D toggleClothes = ContentFinder<Texture2D>.Get("UI/Buttons/ToggleClothes");
	}
}
