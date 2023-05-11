using Pawnmorph.UserInterface.TreeBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Interfaces
{
	internal interface IConfigurableObject : IExposable
	{
		public string Caption { get; }

		public void GenerateMenu(TreeNode_FilterBox node);
	}
}
