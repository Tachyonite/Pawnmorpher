using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.User_Interface.Preview
{
    internal class ThingPreview : Preview
    {
        ThingDef _thing;

        public ThingDef Thing
        {
            get => _thing;
            set => _thing = value;
        }


        public ThingPreview(int height, int width, ThingDef thing)
            : base(height, width)
        {
            _thing = thing;
        }


        public override void OnRefresh()
        {
            _thing.graphic.DrawFromDef(new UnityEngine.Vector3(_previewOffsetX, 0, 0), _rotation, _thing);
        }
    }
}
