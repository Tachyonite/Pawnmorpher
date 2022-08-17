using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.User_Interface.Preview
{
    internal class PawnKindDefPreview : Preview
    {
        PawnKindDef _thing;

        public PawnKindDef Thing
        {
            get => _thing;
            set => _thing = value;
        }


        public PawnKindDefPreview(int height, int width, PawnKindDef thing)
            : base(height, width)
        {
            _thing = thing;
        }


        public override void OnRefresh()
        {
            if (_thing == null)
                return;

            Graphic graphic = null;
            if (_thing.lifeStages != null && _thing.lifeStages.Count > 0)
            {
                var stageIndex = Mathf.FloorToInt(_thing.lifeStages.Count / 2);
                graphic = _thing.lifeStages[stageIndex].bodyGraphicData.Graphic;
            }
            else
            {
                graphic = _thing.race.graphic;
            }

            graphic.DrawFromDef(new UnityEngine.Vector3(_previewOffsetX, 0, 0), _rotation, _thing.race);
        }
    }
}
