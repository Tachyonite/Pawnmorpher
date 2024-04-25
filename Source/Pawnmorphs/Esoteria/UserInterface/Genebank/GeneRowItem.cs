using System.Collections.Generic;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.UserInterface.TableBox;
using Verse;

namespace Pawnmorph.UserInterface.Genebank
{
	internal class GeneRowItem : TableRow<IGenebankEntry>
	{
		public readonly string Label;
		public readonly string StorageSpaceUsed;
		public readonly string StorageSpaceUsedPercentage;
		public readonly int Size;


		public GeneRowItem(IGenebankEntry def, int totalCapacity, string searchString)
			: base(def, searchString)
		{
			Label = def.GetCaption();
			Size = def.GetRequiredStorage();
			StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
			StorageSpaceUsedPercentage = "0%";
			if (totalCapacity > 0)
				StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
		}
	}
}
