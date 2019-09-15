// InitialGraphics.cs modified by Iron Wolf for Pawnmorph on 09/10/2019 6:43 PM
// last updated 09/10/2019  6:43 PM

using System.Text;
using AlienRace;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.GraphicSys
{
    public class InitialGraphicsComp : ThingComp
    {
        private bool _scanned;

        private Vector2 _customDrawSize = Vector2.one;
        private Vector2 _customPortraitDrawSize = Vector2.one;
        private bool _fixedGenderPostSpawn;
        private Color _skinColor;
        private Color _skinColorSecond;
        private Color _hairColorSecond;
        private Color _hairColor;
        private string _crownType;

        public Vector2 CustomDrawSize
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _customDrawSize;
            }
        }

        public Vector2 CustomPortraitDrawSize
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _customPortraitDrawSize;
            }
        }

        public bool FixGenderPostSpawn
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _fixedGenderPostSpawn;
            }
        }

        public Color SkinColor
        {
            get
            {
                if (!_scanned) ScanGraphics();
                return _skinColor;
            }
        }

        public Color HairColor
        {
            get
            {
                if (!_scanned) ScanGraphics();
                if (_hairColor == default)
                    _hairColor = Pawn.story.hairColor; //fix for hair color not being saved in previous saves 

                return _hairColor;
            }
        }

        public Color SkinColorSecond
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _skinColorSecond;
            }
        }

        public Color HairColorSecond
        {
            get
            {
                if (!_scanned) ScanGraphics();
                return _hairColorSecond;
            }
        }

        public string CrownType
        {
            get
            {
                if (!_scanned) ScanGraphics();

                return _crownType;
            }
        }

        private Pawn Pawn => (Pawn) parent;

        public string GetDebugStr()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"{nameof(SkinColor)} {SkinColor}");
            builder.AppendLine($"{nameof(HairColor)} {HairColor}");
            builder.AppendLine($"{nameof(CrownType)} {CrownType}");
            return builder.ToString();
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _customDrawSize, "customDrawSize");
            Scribe_Values.Look(ref _customPortraitDrawSize, "customPortraitDrawSize");
            Scribe_Values.Look(ref _fixedGenderPostSpawn, nameof(FixGenderPostSpawn));
            Scribe_Values.Look(ref _skinColor, "skinColor");
            Scribe_Values.Look(ref _skinColorSecond, "skinColorSecond");
            Scribe_Values.Look(ref _hairColorSecond, "hairColorSecond");
            Scribe_Values.Look(ref _crownType, "crownType");
            Scribe_Values.Look(ref _hairColor, nameof(HairColor));
            Scribe_Values.Look(ref _scanned, nameof(_scanned));
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!_scanned) ScanGraphics();
        }

        /// <summary>
        ///     Restores the alien Comp attached to the parent from the ones stored earlier
        ///     this does not resolve the graphics, that is the job of the caller
        /// </summary>
        public void RestoreGraphics()
        {
            Assert(_scanned, "_scanned");

            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();

            comp.skinColor = SkinColor;
            comp.customDrawSize = CustomDrawSize;
            comp.customPortraitDrawSize = CustomPortraitDrawSize;
            comp.fixGenderPostSpawn = FixGenderPostSpawn;
            comp.skinColorSecond = SkinColorSecond;
            comp.hairColorSecond = HairColorSecond;
            comp.crownType = CrownType;
            comp.hairColorSecond = HairColorSecond;
            ((Pawn) parent).story.hairColor = HairColor;
        }

        private void ScanGraphics()
        {
            _scanned = true;
            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();
            if (comp == null) return;

            _customDrawSize = comp.customDrawSize;
            _customPortraitDrawSize = comp.customPortraitDrawSize;
            _fixedGenderPostSpawn = comp.fixGenderPostSpawn;
            _skinColor = comp.skinColor;
            _skinColorSecond = comp.skinColorSecond;
            _hairColorSecond = comp.hairColorSecond;
            _crownType = comp.crownType;
            _hairColorSecond = Pawn.story.hairColor;
        }
    }
}