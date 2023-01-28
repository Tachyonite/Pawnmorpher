using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.PartPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Genebank.Model
{
	public interface IGenebankEntry : Verse.IExposable
	{
		string GetCaption();
		int GetRequiredStorage();
		bool CanAddToDatabase(ChamberDatabase database, out string reason);
	}
}
