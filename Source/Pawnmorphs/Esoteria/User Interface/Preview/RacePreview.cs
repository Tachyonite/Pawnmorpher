using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.Preview
{
    internal class RacePreview : PreviewBase
    {
        ThingDef_AlienRace _race;
        Pawn _pawn;


        public ThingDef_AlienRace Race
        {
            get => _race;
            set => _race = value;
        }


        public RacePreview(int height, int width, ThingDef_AlienRace race)
            : base(height, width)
        {
            _race = race;
        }

        public override void OnRefresh()
        {
            if (_pawn == null)
            {
                _pawn = new Pawn();
                _pawn.def = _race;
                _pawn.apparel = new Pawn_ApparelTracker(_pawn);
                _pawn.health = new Pawn_HealthTracker(_pawn);
                _pawn.gender = Gender.Male;
                _pawn.story = new Pawn_StoryTracker(_pawn);
                _pawn.story.bodyType = BodyTypeDefOf.Male;
                _pawn.story.crownType = CrownType.Average;
                _pawn.story.hairDef = HairDefOf.Shaved;
                _pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            //_pawn.DrawAt(new Vector3(PREVIEW_POSITION_X, 0, 0));


            return;
        }
    }
}
