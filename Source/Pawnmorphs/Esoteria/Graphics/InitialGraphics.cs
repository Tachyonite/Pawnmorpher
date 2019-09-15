// InitialGraphics.cs modified by Iron Wolf for Pawnmorph on 09/10/2019 6:43 PM
// last updated 09/10/2019  6:43 PM

using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using UnityEngine;
using Verse;

namespace Pawnmorph.GraphicSys
{
    public class InitialGraphicsComp: ThingComp
    {
        private bool _scanned; 

        public Vector2 customDrawSize = Vector2.one;
        public Vector2 customPortraitDrawSize = Vector2.one;
        public bool fixGenderPostSpawn;
        public Color skinColor;
        public Color hairColor; 
        public Color skinColorSecond;
        public Color hairColorSecond;
        public string crownType;


        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!_scanned)
            {
                ScanGraphics();
            }
        }

        public string GetDebugStr()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{nameof(skinColor)} {skinColor}");
            builder.AppendLine($"{nameof(hairColor)} {hairColor}");
            builder.AppendLine($"{nameof(crownType)} {crownType}");
            return builder.ToString(); 
        }

        private void ScanGraphics()
        {
            Log.Warning($"{((Pawn) parent).Name} graphics are being saved ");

            _scanned = true;
            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();
            if (comp == null) return;

            customDrawSize = comp.customDrawSize;
            customPortraitDrawSize = comp.customPortraitDrawSize;
            fixGenderPostSpawn = comp.fixGenderPostSpawn;
            skinColor = comp.skinColor;
            skinColorSecond = comp.skinColorSecond;
            hairColorSecond = comp.hairColorSecond;
            crownType = comp.crownType;
            hairColor = ((Pawn) parent).story.hairColor;
        }

        /// <summary>
        /// Restores the alien Comp attached to the parent from the ones stored earlier
        /// this does not resolve the graphics, that is the job of the caller 
        /// </summary>
        public void RestoreGraphics()
        {
            DebugLogUtils.Assert(_scanned, "_scanned");

            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();

            comp.skinColor = skinColor;
            comp.customDrawSize = customDrawSize;
            comp.customPortraitDrawSize = customPortraitDrawSize;
            comp.fixGenderPostSpawn = fixGenderPostSpawn;
            comp.skinColorSecond = skinColorSecond;
            comp.hairColorSecond = hairColorSecond;
            comp.crownType = crownType;
            comp.hairColorSecond = hairColorSecond;
            ((Pawn) parent).story.hairColor = hairColor; 

        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref customDrawSize, nameof(customDrawSize));
            Scribe_Values.Look(ref customPortraitDrawSize, nameof(customPortraitDrawSize));
            Scribe_Values.Look(ref fixGenderPostSpawn, nameof(fixGenderPostSpawn));
            Scribe_Values.Look(ref skinColor, nameof(skinColor));
            Scribe_Values.Look(ref skinColorSecond, nameof(skinColorSecond));
            Scribe_Values.Look(ref hairColorSecond, nameof(hairColorSecond));
            Scribe_Values.Look(ref crownType, nameof(crownType));
            Scribe_Values.Look(ref _scanned, nameof(_scanned));
        }
    }

    
}