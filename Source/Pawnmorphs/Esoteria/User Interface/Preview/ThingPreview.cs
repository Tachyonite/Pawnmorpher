using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.Preview
{
    internal class ThingPreview : Preview
    {
        private Thing _thing;

        public Thing Thing
        {
            get => _thing;
            protected set => _thing = value;
        }

        public ThingPreview(int height, int width) 
            : base(height, width)
        {
        }

        protected override void OnDraw(Vector3 drawPosition)
        {
            if (_thing == null)
                return;

            _thing.Rotation = _rotation;
            _thing.DrawAt(drawPosition);
        }

        protected override void OnRefresh()
        {

        }
    }
}
