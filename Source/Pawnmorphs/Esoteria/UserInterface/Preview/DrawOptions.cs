using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.UserInterface.Preview
{
    [Flags]
    internal enum DrawOptions
    {
        All = 0,
        Body = 1,
        Head = 2,
        Clothes = 4,
        Headwear = 8,
        BodyAddons = 16,
    }
}
